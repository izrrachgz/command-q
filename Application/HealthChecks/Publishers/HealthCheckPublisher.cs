using Application.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.HealthChecks.Publishers
{
    public class HealthCheckPublisher(ILogger<HealthCheckPublisher> logger) : IHealthCheckPublisher
    {
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
