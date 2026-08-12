using Microsoft.Extensions.Logging;
using Application.Modules.Health.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.Modules.Health.Publishers
{
    /// <summary>
    /// Logs the outcome of a published application health report.
    /// </summary>
    public sealed class HealthCheckPublisher(ILogger<HealthCheckPublisher> logger) : IHealthCheckPublisher
    {
        /// <summary>
        /// Publishes a health report by logging its resulting status.
        /// </summary>
        /// <param name="report">The health report to publish.</param>
        /// <param name="cancellationToken">The token used to cancel publishing.</param>
        /// <returns>A task that completes when the report has been logged.</returns>
        public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
        {
            switch (report.Status)
            {
                case HealthStatus.Unhealthy:
                    RunWhenUnhealthy();
                    break;
                case HealthStatus.Degraded:
                    RunWhenDegraded();
                    break;
                case HealthStatus.Healthy:
                    RunWhenHealthy();
                    break;
                default:
                    RunWhenDefault();
                    break;
            }

            return Task.CompletedTask;
        }

        private Task RunWhenUnhealthy()
        {
            logger.ApplicationUnhealthy();
            return Task.CompletedTask;
        }

        private Task RunWhenDegraded()
        {
            logger.ApplicationDegraded();
            return Task.CompletedTask;
        }

        private Task RunWhenHealthy()
        {
            logger.ApplicationHealthy();
            return Task.CompletedTask;
        }

        private Task RunWhenDefault()
        {
            logger.ApplicationRunning();
            return Task.CompletedTask;
        }
    }
}
