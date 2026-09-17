using Email.Client.Abstractions;
using Email.Client.Models;
using IdentityServer.Modules.Accounts;
using IdentityServer.Modules.Accounts.Contracts.Challenges;
using IdentityServer.Modules.Accounts.Configuration;
using IdentityServer.Modules.Accounts.Services.Email;
using IdentityServer.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NodaTime;

namespace IdentityServer.Test.Modules.Accounts
{
    public sealed class AccountEmailMaintenanceServiceTest
    {
        [Fact]
        public async Task Staged_Email_Should_Be_Protected_And_Reliably_Queued()
        {
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            var databaseOptions = new DbContextOptionsBuilder<DispatchTestDbContext>()
                .UseSqlite(connection)
                .UseOpenIddict()
                .Options;
            await using var database = new DispatchTestDbContext(databaseOptions);
            await database.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
            var now = new DateTimeOffset(2026, 9, 13, 2, 0, 0, TimeSpan.Zero);
            var timeProvider = new TestTimeProvider(now);
            var dataProtection = new EphemeralDataProtectionProvider();
            var accountOptions = Options.Create(CreateOptions());
            var outbox = new CapturingOutbox();
            var user = new IdentityUser
            {
                Id = Guid.NewGuid(),
                UserName = "dispatch-test",
                Email = "dispatch@example.test",
                CreatedUtc = now.UtcDateTime,
                ModifiedUtc = now.UtcDateTime
            };
            var challenge = new IdentityAccountChallenge
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                Purpose = AccountConstants.ChallengePurposes.EmailConfirmation,
                LinkTokenHash = new string('A', 64),
                CodeHash = new string('B', 64),
                ConcurrencyStamp = Guid.NewGuid().ToString("N"),
                CreatedUtc = now.UtcDateTime,
                ExpiresUtc = now.AddMinutes(15).UtcDateTime
            };
            database.AddRange(user, challenge);
            await database.SaveChangesAsync(TestContext.Current.CancellationToken);
            var emailService = new AccountEmailService(
                database,
                new AccountEmailTemplateRenderer(),
                accountOptions,
                timeProvider,
                dataProtection);
            var delivery = new AccountChallengeDelivery(
                challenge.Id,
                "private-link-token",
                "123456",
                challenge.ExpiresUtc);

            await emailService.QueueConfirmation(
                user,
                delivery,
                AccountConstants.Cultures.English,
                TestContext.Current.CancellationToken);

            var dispatch = await database.AccountEmailDispatches.SingleAsync(TestContext.Current.CancellationToken);
            Assert.DoesNotContain("private-link-token", dispatch.ProtectedPayload);
            Assert.DoesNotContain("123456", dispatch.ProtectedPayload);
            Assert.DoesNotContain(user.Email, dispatch.ProtectedPayload);

            var maintenance = new AccountEmailMaintenanceService(
                database,
                outbox,
                accountOptions,
                timeProvider,
                dataProtection,
                NullLogger<AccountEmailMaintenanceService>.Instance);
            await maintenance.ProcessPending(TestContext.Current.CancellationToken);

            Assert.NotNull(outbox.Request);
            Assert.Contains("#token=private-link-token", outbox.Request.Message.HtmlBody);
            Assert.Equal(user.Email, outbox.Request.Message.To.Single().Address);
            Assert.NotNull(dispatch.QueuedUtc);
            Assert.Null(dispatch.ProtectedPayload);

            var revokedChallenge = new IdentityAccountChallenge
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Purpose = AccountConstants.ChallengePurposes.EmailConfirmation,
                LinkTokenHash = new string('C', 64),
                CodeHash = new string('D', 64),
                ConcurrencyStamp = Guid.NewGuid().ToString("N"),
                CreatedUtc = now.UtcDateTime,
                ExpiresUtc = now.AddMinutes(15).UtcDateTime
            };
            database.AccountChallenges.Add(revokedChallenge);
            await database.SaveChangesAsync(TestContext.Current.CancellationToken);
            await emailService.QueueConfirmation(
                user,
                new AccountChallengeDelivery(revokedChallenge.Id, "revoked-token", "654321", revokedChallenge.ExpiresUtc),
                AccountConstants.Cultures.English,
                TestContext.Current.CancellationToken);
            revokedChallenge.ConsumedUtc = now.UtcDateTime;
            await database.SaveChangesAsync(TestContext.Current.CancellationToken);

            await maintenance.ProcessPending(TestContext.Current.CancellationToken);

            var abandonedDispatch = await database.AccountEmailDispatches.SingleAsync(
                value => value.ChallengeId == revokedChallenge.Id,
                TestContext.Current.CancellationToken);
            Assert.Equal(1, outbox.EnqueueCount);
            Assert.NotNull(abandonedDispatch.AbandonedUtc);
            Assert.Null(abandonedDispatch.ProtectedPayload);

            challenge.ConsumedUtc = now.AddDays(-31).UtcDateTime;
            revokedChallenge.ConsumedUtc = now.AddDays(-31).UtcDateTime;
            database.SecurityEvents.AddRange(
                new IdentitySecurityEvent
                {
                    Action = "OldEvent",
                    OccurredUtc = now.AddDays(-91).UtcDateTime
                },
                new IdentitySecurityEvent
                {
                    Action = "RecentEvent",
                    OccurredUtc = now.AddDays(-1).UtcDateTime
                });
            await database.SaveChangesAsync(TestContext.Current.CancellationToken);

            await maintenance.Cleanup(TestContext.Current.CancellationToken);

            Assert.Empty(await database.AccountChallenges.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken));
            Assert.Empty(await database.AccountEmailDispatches.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken));
            Assert.Equal(
                "RecentEvent",
                (await database.SecurityEvents.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken)).Action);
        }

        private static AccountEmailOptions CreateOptions()
        {
            return new AccountEmailOptions
            {
                ApplicationBaseUrl = "https://app.example.test",
                ProfileName = "default",
                ConfirmationLifetimeMinutes = 1440,
                PasswordRecoveryLifetimeMinutes = 15,
                ResetGrantLifetimeMinutes = 15,
                VerificationMaxFailedAttempts = 3,
                PasswordResetMaxFailedAttempts = 3,
                ResendCooldownSeconds = 60,
                DailyDeliveryLimit = 5,
                DispatchPollSeconds = 5,
                DispatchBatchSize = 10,
                DispatchRetryBaseSeconds = 15,
                DispatchRetryMaxMinutes = 60,
                CleanupIntervalMinutes = 60,
                ChallengeRetentionDays = 30,
                SecurityEventRetentionDays = 90
            };
        }

        private sealed class DispatchTestDbContext(DbContextOptions<DispatchTestDbContext> options)
            : IdentityServerDbContext(options);

        private sealed class TestTimeProvider(DateTimeOffset utcNow) : TimeProvider
        {
            public override DateTimeOffset GetUtcNow() => utcNow;
        }

        private sealed class CapturingOutbox : IEmailQueueClient
        {
            public EmailQueueRequest? Request { get; private set; }
            public int EnqueueCount { get; private set; }

            public Task<EmailQueueResult> EnqueueAsync(
                EmailQueueRequest request,
                CancellationToken cancellationToken = default)
            {
                Request = request;
                EnqueueCount++;
                return Task.FromResult(new EmailQueueResult(Guid.NewGuid(), false, DateTime.UtcNow));
            }

            public Task<EmailQueuePageResult> GetPageAsync(
                EmailQueuePageRequest request,
                CancellationToken cancellationToken = default)
            {
                throw new NotSupportedException();
            }

            public Task<EmailQueuePageResult> SearchAsync(
                EmailQueueSearchRequest request,
                CancellationToken cancellationToken = default)
            {
                throw new NotSupportedException();
            }

            public Task<QueuedEmailDetail?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default) =>
                throw new NotSupportedException();
        }
    }
}
