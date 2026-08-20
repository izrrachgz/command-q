using SqlKata.Execution;
using Domain.Modules.Health;
using Persistence.Abstractions;
using Microsoft.Extensions.Logging;
using Application.Modules.Health.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.Modules.Health.Checks
{
    /// <summary>
    /// Verifies that the application database can accept a health-check write.
    /// </summary>
    internal sealed class DatabaseWriteHealthCheck(IWritableRepository repository, ILogger<DatabaseWriteHealthCheck> logger) : IHealthCheck
    {
        private const string HealthyMessage = "Application is able to write health check data.";
        private const string UnhealthyMessage = "Application is unable to write health check data.";

        /// <inheritdoc />
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return await CheckAsync(context.Registration.FailureStatus, cancellationToken);
        }

        /// <summary>
        /// Runs the database-write probe using the default unhealthy failure status.
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
                var now = DateTime.UtcNow;
                var values = new
                {
                    Created = now,
                    Modified = now
                };

                int affected = await repository.Db.Query(nameof(HealthCheckLog))
                    .InsertAsync(values, cancellationToken: cancellationToken);

                if (affected == 0)
                {
                    logger.DependencyCheckFailed("database write");

                    return new HealthCheckResult(failureStatus, UnhealthyMessage);
                }

                logger.DependencyCheckSucceeded("database write");

                return HealthCheckResult.Healthy(HealthyMessage);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.DependencyCheckFailed("database write");

                return new HealthCheckResult(failureStatus, UnhealthyMessage, exception);
            }
        }
    }
}
