using NodaTime;
using Email.Outbox.Models;
using Email.Outbox.Abstractions;
using Microsoft.Extensions.Hosting;
using Email.Outbox.Persistence.Health;
using Microsoft.Extensions.Configuration;
using Email.Outbox.Persistence.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Email.Outbox.Test.Persistence
{
    public sealed class EmailOutboxPersistenceTest
    {
        [Fact]
        public async Task SqLite_Store_Should_Migrate_Deduplicate_Claim_And_Complete()
        {
            string testRoot = Path.Combine(Path.GetTempPath(), $"command-q-outbox-{Guid.NewGuid():N}");
            Directory.CreateDirectory(testRoot);
            string databasePath = Path.Combine(testRoot, "outbox.db");

            try
            {
                var settings = new HostApplicationBuilderSettings
                {
                    ContentRootPath = testRoot,
                    EnvironmentName = Environments.Development
                };
                var builder = Host.CreateApplicationBuilder(settings);
                builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["TestHost:OutboxPersistence:Engine"] = "SqLite",
                    ["TestHost:OutboxPersistence:ConnectionString"] = $"Data Source={databasePath};Version=3;Foreign Keys=True;"
                });
                builder.Services.AddEmailOutboxPersistence(
                    builder.Configuration.GetRequiredSection("TestHost:OutboxPersistence"));
                builder.Services.AddHealthChecks()
                    .AddEmailOutboxPersistenceHealthCheck("email-outbox-persistence");

                using var host = builder.Build();
                await host.RunEmailOutboxMigrations(TestContext.Current.CancellationToken);
                var healthCheckService = host.Services.GetRequiredService<HealthCheckService>();
                var healthReport = await healthCheckService.CheckHealthAsync(TestContext.Current.CancellationToken);
                Assert.Equal(HealthStatus.Healthy, healthReport.Status);
                Assert.Contains("email-outbox-persistence", healthReport.Entries.Keys);

                var store = host.Services.GetRequiredService<IEmailOutboxStore>();
                Instant utcNow = SystemClock.Instance.GetCurrentInstant() + Duration.FromNanoseconds(789);
                Instant persistedUtcNow = Time.UtcTime.TruncateToPersistencePrecision(utcNow);
                var record = new EmailOutboxRecord(
                    Guid.NewGuid(),
                    "{\"message\":\"test\"}",
                    "order-100-email",
                    "account-1",
                    "correlation-1",
                    0,
                    utcNow,
                    utcNow,
                    TraceParent: "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
                    TraceState: "vendor=value");

                var first = await store.EnqueueAsync(record, TestContext.Current.CancellationToken);
                var duplicate = await store.EnqueueAsync(
                    record with { Id = Guid.NewGuid() },
                    TestContext.Current.CancellationToken);

                Assert.False(first.WasDuplicate);
                Assert.True(duplicate.WasDuplicate);
                Assert.Equal(first.Id, duplicate.Id);
                Assert.Equal(persistedUtcNow, first.CreatedUtc);
                Assert.Equal(first.CreatedUtc, duplicate.CreatedUtc);

                var claimed = await store.ClaimPendingAsync(
                    new EmailOutboxClaimRequest(
                        10,
                        "worker-one",
                        Duration.FromMinutes(5),
                        utcNow + Duration.FromSeconds(1)),
                    TestContext.Current.CancellationToken);
                var claimedRecord = Assert.Single(claimed);
                Assert.Equal(persistedUtcNow, claimedRecord.CreatedUtc);
                Assert.Equal(record.TraceParent, claimedRecord.TraceParent);
                Assert.Equal(record.TraceState, claimedRecord.TraceState);

                var secondClaim = await store.ClaimPendingAsync(
                    new EmailOutboxClaimRequest(
                        10,
                        "worker-two",
                        Duration.FromMinutes(5),
                        utcNow + Duration.FromSeconds(2)),
                    TestContext.Current.CancellationToken);
                Assert.Empty(secondClaim);

                bool markedSent = await store.MarkSentAsync(
                    new EmailOutboxSentUpdate(
                        claimedRecord.Id,
                        claimedRecord.LockId,
                        1,
                        "message-id",
                        "Accepted",
                        utcNow + Duration.FromSeconds(3)),
                    TestContext.Current.CancellationToken);
                Assert.True(markedSent);

                var afterCompletion = await store.ClaimPendingAsync(
                    new EmailOutboxClaimRequest(
                        10,
                        "worker-three",
                        Duration.FromMinutes(5),
                        utcNow + Duration.FromMinutes(10)),
                    TestContext.Current.CancellationToken);
                Assert.Empty(afterCompletion);

                await host.RunEmailOutboxMigrations(TestContext.Current.CancellationToken);
            }
            finally
            {
                if (Directory.Exists(testRoot))
                {
                    Directory.Delete(testRoot, recursive: true);
                }
            }
        }
    }
}
