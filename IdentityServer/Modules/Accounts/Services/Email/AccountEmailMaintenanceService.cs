using System.Security.Cryptography;
using System.Text.Json;
using IdentityServer.Modules.Accounts.Abstractions.Email;
using IdentityServer.Modules.Accounts.Payloads;
using Email.Client.Abstractions;
using Email.Client.Models;
using IdentityServer.Modules.Accounts.Configuration;
using IdentityServer.Modules.Accounts.Logging;
using IdentityServer.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Time.Extensions;

namespace IdentityServer.Modules.Accounts.Services.Email
{
    /// <summary>
    /// Relays protected account-email dispatches from the IdentityServer database to the external email queue.
    /// </summary>
    /// <remarks>
    /// Due dispatches are decrypted and submitted through <see cref="IEmailQueueClient"/>. Successful payloads are erased,
    /// transient failures receive exponential backoff, and invalid, consumed, or expired dispatches are abandoned. The service
    /// also deletes challenges and security events after their configured retention windows.
    /// </remarks>
    internal sealed class AccountEmailMaintenanceService : IAccountEmailMaintenanceService
    {
        private readonly IdentityServerDbContext database;
        private readonly IEmailQueueClient emailQueueClient;
        private readonly AccountEmailOptions options;
        private readonly TimeProvider timeProvider;
        private readonly ILogger<AccountEmailMaintenanceService> logger;
        private readonly IDataProtector protector;

        public AccountEmailMaintenanceService(
            IdentityServerDbContext database,
            IEmailQueueClient emailQueueClient,
            IOptions<AccountEmailOptions> options,
            TimeProvider timeProvider,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<AccountEmailMaintenanceService> logger)
        {
            this.database = database;
            this.emailQueueClient = emailQueueClient;
            this.options = options.Value;
            this.timeProvider = timeProvider;
            this.logger = logger;
            protector = dataProtectionProvider.CreateProtector(AccountEmailDispatchProtection.Purpose);
        }

        /// <inheritdoc />
        public async Task ProcessPending(CancellationToken cancellationToken)
        {
            var now = timeProvider.GetCurrentUtcDateTime();
            var dispatches = await database.AccountEmailDispatches
                .Include(value => value.Challenge)
                .Where(value => value.QueuedUtc == null &&
                    value.AbandonedUtc == null &&
                    value.NextAttemptUtc <= now)
                .OrderBy(value => value.NextAttemptUtc)
                .Take(options.DispatchBatchSize)
                .ToListAsync(cancellationToken);

            foreach (var dispatch in dispatches)
            {
                if (dispatch.Challenge.ConsumedUtc.HasValue ||
                    dispatch.Challenge.ExpiresUtc <= now ||
                    string.IsNullOrWhiteSpace(dispatch.ProtectedPayload))
                {
                    await Abandon(dispatch, now, cancellationToken);
                    continue;
                }

                AccountEmailDispatchPayload? payload;

                try
                {
                    payload = JsonSerializer.Deserialize<AccountEmailDispatchPayload>(
                        protector.Unprotect(dispatch.ProtectedPayload));
                }
                catch (Exception exception) when (exception is CryptographicException or JsonException)
                {
                    logger.AccountEmailDispatchInvalid(exception, dispatch.Id);
                    await Abandon(dispatch, now, cancellationToken);
                    continue;
                }

                if (payload is null)
                {
                    await Abandon(dispatch, now, cancellationToken);
                    continue;
                }

                try
                {
                    await emailQueueClient.EnqueueAsync(CreateRequest(payload), cancellationToken);
                    dispatch.QueuedUtc = now;
                    dispatch.ProtectedPayload = null;
                    dispatch.ConcurrencyStamp = Guid.NewGuid().ToString("N");
                    await SaveDispatch(dispatch, cancellationToken);
                    logger.AccountEmailDispatchQueued(dispatch.Id, dispatch.ChallengeId);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    dispatch.AttemptCount++;
                    dispatch.NextAttemptUtc = now.Add(CalculateRetryDelay(dispatch.AttemptCount));
                    dispatch.ConcurrencyStamp = Guid.NewGuid().ToString("N");
                    await SaveDispatch(dispatch, cancellationToken);
                    logger.AccountEmailDispatchRetry(exception, dispatch.Id, dispatch.AttemptCount, dispatch.NextAttemptUtc);
                }
            }
        }

        /// <inheritdoc />
        public async Task Cleanup(CancellationToken cancellationToken)
        {
            var now = timeProvider.GetCurrentUtcDateTime();
            var challengeCutoff = now.AddDays(-options.ChallengeRetentionDays);
            var securityEventCutoff = now.AddDays(-options.SecurityEventRetentionDays);
            int challengeCount;
            int securityEventCount;

            if (database.Database.IsRelational())
            {
                challengeCount = await database.AccountChallenges
                    .Where(value =>
                        (value.ConsumedUtc.HasValue && value.ConsumedUtc < challengeCutoff) ||
                        (!value.ConsumedUtc.HasValue && value.ExpiresUtc < challengeCutoff))
                    .ExecuteDeleteAsync(cancellationToken);
                securityEventCount = await database.SecurityEvents
                    .Where(value => value.OccurredUtc < securityEventCutoff)
                    .ExecuteDeleteAsync(cancellationToken);
            }
            else
            {
                var challenges = await database.AccountChallenges
                    .Where(value =>
                        (value.ConsumedUtc.HasValue && value.ConsumedUtc < challengeCutoff) ||
                        (!value.ConsumedUtc.HasValue && value.ExpiresUtc < challengeCutoff))
                    .ToListAsync(cancellationToken);
                var securityEvents = await database.SecurityEvents
                    .Where(value => value.OccurredUtc < securityEventCutoff)
                    .ToListAsync(cancellationToken);
                challengeCount = challenges.Count;
                securityEventCount = securityEvents.Count;
                database.RemoveRange(challenges);
                database.RemoveRange(securityEvents);
                await database.SaveChangesAsync(cancellationToken);
            }

            if (challengeCount > 0 || securityEventCount > 0)
            {
                logger.AccountDataCleanedUp(challengeCount, securityEventCount);
            }
        }

        private static EmailQueueRequest CreateRequest(AccountEmailDispatchPayload payload)
        {
            return new EmailQueueRequest
            {
                Message = new EmailMessage
                {
                    To = [new EmailAddress(payload.RecipientAddress, payload.RecipientName)],
                    Subject = payload.Subject,
                    HtmlBody = payload.HtmlBody,
                    TextBody = payload.TextBody
                },
                ProfileName = payload.ProfileName,
                IdempotencyKey = payload.IdempotencyKey,
                AccountReference = payload.AccountReference
            };
        }

        private async Task Abandon(
            IdentityAccountEmailDispatch dispatch,
            DateTime now,
            CancellationToken cancellationToken)
        {
            dispatch.AbandonedUtc = now;
            dispatch.ProtectedPayload = null;
            dispatch.ConcurrencyStamp = Guid.NewGuid().ToString("N");
            await SaveDispatch(dispatch, cancellationToken);
        }

        private async Task SaveDispatch(IdentityAccountEmailDispatch dispatch, CancellationToken cancellationToken)
        {
            try
            {
                await database.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                database.Entry(dispatch).State = EntityState.Detached;
            }
        }

        private TimeSpan CalculateRetryDelay(int attemptCount)
        {
            var exponent = Math.Min(attemptCount - 1, 20);
            var seconds = options.DispatchRetryBaseSeconds * Math.Pow(2, exponent);
            return TimeSpan.FromSeconds(Math.Min(seconds, TimeSpan.FromMinutes(options.DispatchRetryMaxMinutes).TotalSeconds));
        }
    }
}
