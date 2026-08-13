using Application.Modules.Health.Checks;
using Microsoft.Extensions.Configuration;
using Application.Modules.Health.Publishers;
using Application.Modules.Health.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.Modules.Health.Extensions
{
    /// <summary>
    /// Provides application health-check service registration.
    /// </summary>
    public static class HealthCheckServiceExtensions
    {
        /// <summary>
        /// Registers application liveness, readiness and diagnostic health checks.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configuration">The host-relative configuration section.</param>
        /// <returns>The configured service collection.</returns>
        public static IServiceCollection ConfigureAppHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection healthCheckSection = configuration.GetSection(
                HealthCheckConstants.ConfigurationSection);

            services.AddOptions<HealthCheckConfigurationOptions>()
                .Bind(healthCheckSection)
                .Validate(options =>
                    Uri.TryCreate(options.UrlCheckInternetAccess, UriKind.Absolute, out _),
                    "HealthCheck:UrlCheckInternetAccess must be an absolute URI.")
                .Validate(options => options.PublishIntervalSeconds > 0,
                    "HealthCheck:PublishIntervalSeconds must be greater than zero.")
                .ValidateOnStart();

            services.AddHealthChecks()
                .AddCheck(
                    HealthCheckConstants.SelfName,
                    () => HealthCheckResult.Healthy(HealthCheckConstants.ApplicationRunningMessage),
                    tags: [HealthCheckConstants.LiveTag])
                .AddTypeActivatedCheck<DatabaseReadHealthCheck>(
                    HealthCheckConstants.DatabaseReadName,
                    failureStatus: HealthStatus.Unhealthy,
                    tags: [HealthCheckConstants.ReadyTag, HealthCheckConstants.StartupTag, HealthCheckConstants.PublishTag])
                .AddTypeActivatedCheck<DatabaseWriteHealthCheck>(
                    HealthCheckConstants.DatabaseWriteName,
                    failureStatus: HealthStatus.Unhealthy,
                    tags: [HealthCheckConstants.StartupTag, HealthCheckConstants.PublishTag])
                .AddTypeActivatedCheck<LocalStorageWriteHealthCheck>(
                    HealthCheckConstants.LocalStorageWriteName,
                    failureStatus: HealthStatus.Unhealthy,
                    tags: [HealthCheckConstants.StartupTag, HealthCheckConstants.PublishTag])
                .AddTypeActivatedCheck<InternetAccessHealthCheck>(
                    HealthCheckConstants.InternetAccessName,
                    failureStatus: HealthStatus.Unhealthy,
                    tags: [HealthCheckConstants.StartupTag, HealthCheckConstants.PublishTag]);

            services.AddSingleton<HealthCheckPublisher>();
            services.AddHostedService<DynamicHealthCheckPublisherService>();

            return services;
        }
    }
}
