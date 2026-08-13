using SqlKata.Execution;
using Email.Outbox.Persistence.Storage;
using Email.Outbox.Persistence.Constants;
using Email.Outbox.Persistence.Migrations.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Email.Outbox.Persistence.Health
{
    /// <summary>
    /// Verifies that the configured email outbox persistence store can be queried.
    /// </summary>
    internal sealed class EmailOutboxPersistenceHealthCheck(EmailOutboxRepositoryFactory repositoryFactory) : IHealthCheck
    {
        /// <inheritdoc />
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var repository = repositoryFactory.Create();
                _ = await repository.Db.Query(EmailOutboxPersistenceConstants.MigrationTable)
                    .Select(nameof(EmailOutboxMigrationRecord.MigrationId))
                    .Limit(1)
                    .GetAsync<EmailOutboxMigrationRecord>(cancellationToken: cancellationToken);

                return HealthCheckResult.Healthy("Email outbox persistence is reachable.");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                return new HealthCheckResult(
                    context.Registration.FailureStatus,
                    "Email outbox persistence is not reachable.",
                    exception);
            }
        }
    }
}
