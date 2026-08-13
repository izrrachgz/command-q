using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Application.Modules.Health.Publishers;
using Application.Modules.Health.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.Modules.Health.Extensions
{
    /// <summary>
    /// Provides startup execution for application diagnostic health checks.
    /// </summary>
    public static class StartupHealthCheckExtensions
    {
        /// <summary>
        /// Runs startup-tagged health checks when configured to do so.
        /// </summary>
        /// <param name="host">The host whose registered health checks should be evaluated.</param>
        /// <param name="cancellationToken">The token used to cancel health-check execution.</param>
        /// <returns>A task that completes when startup health checks have finished or have been skipped.</returns>
        public static async Task RunStartupHealthChecks(this IHost host, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(host);

            var options = host.Services.GetRequiredService<IOptions<HealthCheckConfigurationOptions>>().Value;

            if (!options.RunOnStartup)
            {
                return;
            }

            var healthCheckService = host.Services.GetRequiredService<HealthCheckService>();
            var publisher = host.Services.GetRequiredService<HealthCheckPublisher>();
            var report = await healthCheckService.CheckHealthAsync(
                check => check.Tags.Contains(HealthCheckConstants.StartupTag),
                cancellationToken);

            await publisher.PublishAsync(report, cancellationToken);

            if (options.FailStartupOnUnhealthy && report.Status != HealthStatus.Healthy)
            {
                throw new InvalidOperationException($"Startup health checks failed with status '{report.Status}'.");
            }
        }
    }
}
