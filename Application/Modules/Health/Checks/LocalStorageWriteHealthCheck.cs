using Microsoft.Extensions.Logging;
using Application.Modules.Health.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.Modules.Health.Checks
{
    /// <summary>
    /// Verifies that the application can write a small health-check file to local storage.
    /// </summary>
    internal sealed class LocalStorageWriteHealthCheck(ILogger<LocalStorageWriteHealthCheck> logger) : IHealthCheck
    {
        private const string FileName = "HealthCheckLog.log";
        private const string HealthyMessage = "Application is able to write health check data to local storage.";
        private const string UnhealthyMessage = "Application is unable to write health check data to local storage.";

        /// <inheritdoc />
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return await CheckAsync(context.Registration.FailureStatus, cancellationToken);
        }

        /// <summary>
        /// Runs the local-storage write probe using the default unhealthy failure status.
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
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);
                string[] content = [DateTime.UtcNow.ToString("O")];

                await File.AppendAllLinesAsync(filePath, content, cancellationToken);

                logger.DependencyCheckSucceeded("storage write");

                return HealthCheckResult.Healthy(HealthyMessage);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.DependencyCheckFailed("storage write");

                return new HealthCheckResult(failureStatus, UnhealthyMessage, exception);
            }
        }
    }
}
