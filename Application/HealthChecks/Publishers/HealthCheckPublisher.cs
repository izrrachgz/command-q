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
            logger.LogCritical($@"Application is unhealthy");
            return Task.CompletedTask;
        }

        private Task RunWhenDegraded()
        {
            logger.LogWarning($@"Application is Degraded");
            return Task.CompletedTask;
        }

        private Task RunWhenHealthy()
        {
            logger.LogInformation($@"Application is Healthy.");
            return Task.CompletedTask;
        }

        private Task RunWhenDefault()
        {
            logger.LogInformation($@"Application is running.");
            return Task.CompletedTask;
        }
    }
}
