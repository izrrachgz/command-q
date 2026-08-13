using Domain.Modules.Health;
using Persistence.Abstractions;
using Microsoft.Extensions.Logging;
using Application.Modules.Health.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.Modules.Health.Checks
{
    /// <summary>
    /// Verifies that the application database can be queried.
    /// </summary>
    internal sealed class DatabaseReadHealthCheck(
        IReadOnlyRepository readOnlyRepository,
        ILogger<DatabaseReadHealthCheck> logger) : IHealthCheck
    {
        private const string HealthyMessage = "Application is able to query health check data.";
        private const string UnhealthyMessage = "Application is unable to query health check data.";

        /// <inheritdoc />
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return await CheckAsync(context.Registration.FailureStatus, cancellationToken);
        }

        /// <summary>
        /// Runs the database-read probe using the default unhealthy failure status.
        /// </summary>
        /// <param name="cancellationToken">The token used to cancel the probe.</param>
        /// <returns>The resulting health-check status.</returns>
        internal async Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken = default)
        {
            return await CheckAsync(HealthStatus.Unhealthy, cancellationToken);
        }

        private async Task<HealthCheckResult> CheckAsync(HealthStatus failureStatus, CancellationToken cancellationToken)
        {
            try
            {
                var query = readOnlyRepository.Query(nameof(HealthCheckLog))
                    .WhereNull(nameof(HealthCheckLog.Deleted))
                    .OrderByDesc(nameof(HealthCheckLog.Modified))
                    .Limit(1);

                HealthCheckLog? record = await readOnlyRepository.FirstOrDefaultAsync<HealthCheckLog>(query, cancellationToken);

                logger.DependencyCheckSucceeded("database read");

                if (record is null)
                {
                    return HealthCheckResult.Healthy(HealthyMessage);
                }

                string detail = $"Application is able to read health check data, last record modified at: {record.Modified:G}.";

                return HealthCheckResult.Healthy(detail);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.DependencyCheckFailed("database read");

                return new HealthCheckResult(failureStatus, UnhealthyMessage, exception);
            }
        }
    }
}
