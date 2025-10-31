using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.HealthChecks.Publishers
{
    public class HealthCheckPublisher : IHealthCheckPublisher
    {
        private readonly ILogger<HealthCheckPublisher> _logger;

        public HealthCheckPublisher(ILogger<HealthCheckPublisher> logger)
        {
            _logger = logger;
        }

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
            _logger.LogCritical($@"Application is unhealthy");
            return Task.CompletedTask;
        }

        private Task RunWhenDegraded()
        {
            _logger.LogWarning($@"Application is Degraded");
            return Task.CompletedTask;
        }

        private Task RunWhenHealthy()
        {
            _logger.LogInformation($@"Application is Healthy.");
            return Task.CompletedTask;
        }

        private Task RunWhenDefault()
        {
            _logger.LogInformation($@"Application is running.");
            return Task.CompletedTask;
        }
    }
}
