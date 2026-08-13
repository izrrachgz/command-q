using Application.Modules.Health.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.Modules.Health.Publishers
{
    /// <summary>
    /// Publishes health reports using the latest enablement and interval settings.
    /// </summary>
    internal sealed class DynamicHealthCheckPublisherService(
        HealthCheckService healthCheckService,
        HealthCheckPublisher publisher,
        IOptionsMonitor<HealthCheckConfigurationOptions> options) : BackgroundService
    {
        /// <summary>
        /// Runs the reload-aware health publishing loop until the host stops.
        /// </summary>
        /// <param name="stoppingToken">Signals that the host is stopping.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var currentOptions = options.CurrentValue;

                if (currentOptions.ShouldPublishReports)
                {
                    var report = await healthCheckService.CheckHealthAsync(
                        check => check.Tags.Contains(HealthCheckConstants.PublishTag),
                        stoppingToken);

                    await publisher.PublishAsync(report, stoppingToken);
                }

                int intervalSeconds = Math.Max(1, currentOptions.PublishIntervalSeconds);
                await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
            }
        }
    }
}
