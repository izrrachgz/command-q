using System.Security.Cryptography;
using System.Data;
using IdentityServer.Modules.Accounts.Configuration;
using IdentityServer.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using IdentityServer.Modules.Accounts.Abstractions.Challenges;
using IdentityServer.Modules.Accounts.Abstractions.Email;
using IdentityServer.Modules.Accounts.Abstractions.Recovery;
using IdentityServer.Modules.Accounts.Abstractions.Security;
using IdentityServer.Modules.Accounts.Contracts.Challenges;
using IdentityServer.Modules.Accounts.Contracts.Recovery;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Time.Extensions;

namespace IdentityServer.Modules.Accounts.Services.Recovery
{
    /// <summary>
    /// Coordinates confirmation and password-recovery workflows across Identity, persisted challenges, email staging, and security events.
    /// </summary>
    /// <remarks>
    /// The service avoids account-existence disclosure, issues time-limited reset grants only after successful challenge verification,
    /// and applies an administrator-required lock when the password submission budget is exhausted.
    /// </remarks>
    internal sealed class AccountRecoveryService : IAccountRecoveryService
    {
        private const string ResetGrantProtectorPurpose = "IdentityServer.AccountRecovery.ResetGrant.v1";
        private readonly UserManager<IdentityUser> userManager;
        private readonly IdentityServerDbContext database;
        private readonly IAccountChallengeService challenges;
        private readonly IAccountEmailService emails;
        private readonly IIdentitySecurityEventRecorder securityEvents;
        private readonly TimeProvider timeProvider;
        private readonly AccountEmailOptions options;
        private readonly ITimeLimitedDataProtector resetGrantProtector;

        public AccountRecoveryService(
            UserManager<IdentityUser> userManager,
            IdentityServerDbContext database,
            IAccountChallengeService challenges,
            IAccountEmailService emails,
            IIdentitySecurityEventRecorder securityEvents,
            TimeProvider timeProvider,
            IOptions<AccountEmailOptions> options,
            IDataProtectionProvider dataProtectionProvider)
        {
            this.userManager = userManager;
            this.database = database;
            this.challenges = challenges;
            this.emails = emails;
            this.securityEvents = securityEvents;
            this.timeProvider = timeProvider;
            this.options = options.Value;
            resetGrantProtector = dataProtectionProvider
                .CreateProtector(ResetGrantProtectorPurpose)
                .ToTimeLimitedDataProtector();
        }

        /// <inheritdoc />
        public async Task<bool> QueueEmailConfirmation(IdentityUser user, string? culture, CancellationToken cancellationToken)
        {
            return await ExecuteTransactional(async () =>
            {
                var challenge = await challenges.Create(
                    user,
                    AccountConstants.ChallengePurposes.EmailConfirmation,
                    TimeSpan.FromMinutes(options.ConfirmationLifetimeMinutes),
                    cancellationToken);

                if (challenge is null)
                {
                    return false;
                }

                await emails.QueueConfirmation(user, challenge, culture, cancellationToken);
                await securityEvents.Record(
                    AccountConstants.Actions.EmailConfirmationRequested,
                    user.Id,
                    cancellationToken: cancellationToken);
                return true;
            }, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RequestEmailConfirmation(string email, string? culture, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(email.Trim());

            if (user is null || user.EmailConfirmed || await IsLocked(user))
            {
                return;
            }

            await QueueEmailConfirmation(user, culture, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<AccountVerificationResult> ConfirmEmail(
            string linkToken,
            string code,
            CancellationToken cancellationToken)
        {
            return await ExecuteTransactional(async () =>
            {
                var verification = await challenges.Verify(
                    linkToken,
                    code,
                    AccountConstants.ChallengePurposes.EmailConfirmation,
                    cancellationToken);

                if (!verification.Succeeded || verification.VerifiedChallenge is null)
                {
                    await RecordLockoutIfRequired(verification, cancellationToken);
                    return new AccountVerificationResult(false, verification.LockedOut);
                }

                var challenge = verification.VerifiedChallenge.Challenge;
                var user = verification.VerifiedChallenge.User;

                if (!user.EmailConfirmed)
                {
                    challenge.ConsumedUtc = timeProvider.GetCurrentUtcDateTime();
                    challenge.ConcurrencyStamp = Guid.NewGuid().ToString("N");
                    user.ModifiedUtc = timeProvider.GetCurrentUtcDateTime();
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var result = await userManager.ConfirmEmailAsync(user, token);

                    if (!result.Succeeded)
                    {
                        return new AccountVerificationResult(false, false);
                    }
                }
                else
                {
                    await challenges.Consume(challenge, cancellationToken);
                }

                await securityEvents.Record(
                    AccountConstants.Actions.EmailConfirmed,
                    user.Id,
                    cancellationToken: cancellationToken);

                return new AccountVerificationResult(true, false);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RequestPasswordRecovery(string email, string? culture, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(email.Trim());

            if (user is null || !user.EmailConfirmed || await IsLocked(user))
            {
                return;
            }

            await ExecuteTransactional(async () =>
            {
                var challenge = await challenges.Create(
                    user,
                    AccountConstants.ChallengePurposes.PasswordRecovery,
                    TimeSpan.FromMinutes(options.PasswordRecoveryLifetimeMinutes),
                    cancellationToken);

                if (challenge is null)
                {
                    return false;
                }

                await emails.QueuePasswordRecovery(user, challenge, culture, cancellationToken);
                await securityEvents.Record(
                    AccountConstants.Actions.PasswordRecoveryRequested,
                    user.Id,
                    cancellationToken: cancellationToken);
                return true;
            }, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<PasswordRecoveryVerificationResult> VerifyPasswordRecovery(
            string linkToken,
            string code,
            CancellationToken cancellationToken)
        {
            var verification = await challenges.Verify(
                linkToken,
                code,
                AccountConstants.ChallengePurposes.PasswordRecovery,
                cancellationToken);

            if (!verification.Succeeded || verification.VerifiedChallenge is null)
            {
                await RecordLockoutIfRequired(verification, cancellationToken);
                return new(false, verification.LockedOut, null, null);
            }

            var verified = verification.VerifiedChallenge;
            var lifetime = TimeSpan.FromMinutes(options.ResetGrantLifetimeMinutes);
            var expiresUtc = timeProvider.GetCurrentUtcDateTime().Add(lifetime);
            var payload = $"{verified.Challenge.Id:N}:{verified.User.Id:N}";
            var resetGrant = resetGrantProtector.Protect(payload, lifetime);

            await securityEvents.Record(
                AccountConstants.Actions.PasswordRecoveryVerified,
                verified.User.Id,
                cancellationToken: cancellationToken);

            return new(true, false, resetGrant, expiresUtc);
        }

        /// <inheritdoc />
        public async Task<PasswordResetResult> ResetPassword(
            string resetGrant,
            string newPassword,
            CancellationToken cancellationToken)
        {
            if (!TryReadGrant(resetGrant, out var challengeId, out var userId))
            {
                return InvalidReset();
            }

            return await ExecuteTransactional(async () =>
            {
                var now = timeProvider.GetCurrentUtcDateTime();
                var challenge = await database.AccountChallenges
                    .Include(value => value.User)
                    .SingleOrDefaultAsync(value =>
                        value.Id == challengeId &&
                        value.UserId == userId &&
                        value.Purpose == AccountConstants.ChallengePurposes.PasswordRecovery,
                        cancellationToken);

                if (challenge is null ||
                    !challenge.VerifiedUtc.HasValue ||
                    challenge.ConsumedUtc.HasValue ||
                    challenge.ExpiresUtc <= now ||
                    await IsLocked(challenge.User))
                {
                    return InvalidReset(challenge?.User is not null && await IsLocked(challenge.User));
                }

                var passwordToken = await userManager.GeneratePasswordResetTokenAsync(challenge.User);
                challenge.ConsumedUtc = now;
                challenge.ConcurrencyStamp = Guid.NewGuid().ToString("N");
                challenge.User.ModifiedUtc = now;
                var result = await userManager.ResetPasswordAsync(challenge.User, passwordToken, newPassword);

                if (!result.Succeeded)
                {
                    if (result.Errors.Any(error => error.Code == nameof(IdentityErrorDescriber.ConcurrencyFailure)))
                    {
                        return InvalidReset();
                    }

                    challenge.ConsumedUtc = null;
                    var lockedOut = await challenges.RegisterPasswordResetFailure(challenge, challenge.User, cancellationToken);

                    if (lockedOut)
                    {
                        await securityEvents.Record(
                            AccountConstants.Actions.AccountLockedByRecovery,
                            challenge.User.Id,
                            cancellationToken: cancellationToken);
                    }

                    return new PasswordResetResult(result, lockedOut);
                }

                await userManager.ResetAccessFailedCountAsync(challenge.User);
                await securityEvents.Record(
                    AccountConstants.Actions.PasswordReset,
                    challenge.User.Id,
                    cancellationToken: cancellationToken);

                return new PasswordResetResult(IdentityResult.Success, false);
            }, cancellationToken);
        }

        private Task<bool> IsLocked(IdentityUser user)
        {
            return user.RequiresAdminUnlock
                ? Task.FromResult(true)
                : userManager.IsLockedOutAsync(user);
        }

        private async Task<T> ExecuteTransactional<T>(Func<Task<T>> operation, CancellationToken cancellationToken)
        {
            if (!database.Database.IsRelational())
            {
                return await operation();
            }

            var strategy = database.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await database.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);
                var result = await operation();
                await transaction.CommitAsync(cancellationToken);
                return result;
            });
        }

        private bool TryReadGrant(string resetGrant, out Guid challengeId, out Guid userId)
        {
            challengeId = default;
            userId = default;

            try
            {
                var payload = resetGrantProtector.Unprotect(resetGrant, out _);
                var parts = payload.Split(':', 2, StringSplitOptions.TrimEntries);
                return parts.Length == 2 &&
                    Guid.TryParseExact(parts[0], "N", out challengeId) &&
                    Guid.TryParseExact(parts[1], "N", out userId);
            }
            catch (CryptographicException)
            {
                return false;
            }
        }

        private async Task RecordLockoutIfRequired(
            AccountChallengeVerificationResult verification,
            CancellationToken cancellationToken)
        {
            if (verification.LockedOut && verification.UserId.HasValue)
            {
                await securityEvents.Record(
                    AccountConstants.Actions.AccountLockedByRecovery,
                    verification.UserId.Value,
                    cancellationToken: cancellationToken);
            }
        }

        private static PasswordResetResult InvalidReset(bool lockedOut = false)
        {
            return new PasswordResetResult(
                IdentityResult.Failed(new IdentityError
                {
                    Code = "InvalidPasswordRecovery",
                    Description = "The password recovery authorization is invalid or expired."
                }),
                lockedOut);
        }
    }
}
