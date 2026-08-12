using Application.Modules.Health.Queries;
using Application.Modules.Health.Commands;
using Application.Modules.Health.Publishers;
using Application.Modules.Health.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.Modules.Health.Extensions
{
    public static class HealthCheckServiceExtensions
    {

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
                .AddCheck(HealthCheckConstants.ApplicationRunningName, () => HealthCheckResult.Healthy(HealthCheckConstants.ApplicationRunningMessage), tags: [HealthCheckConstants.LiveTag])
                .AddTypeActivatedCheck<SaveLocalStorageDataCommandHandler>(@"Can Write On Local Storage", failureStatus: HealthStatus.Unhealthy, tags: [HealthCheckConstants.ReadyTag])
                .AddTypeActivatedCheck<CheckInternetConnectionQueryHandler>(@"Can Access Internet", failureStatus: HealthStatus.Unhealthy, tags: [HealthCheckConstants.ReadyTag])
                .AddTypeActivatedCheck<SaveDatabaseDataCommandHandler>(@"Can Write On Database", failureStatus: HealthStatus.Unhealthy, tags: [HealthCheckConstants.ReadyTag])
                .AddTypeActivatedCheck<GetDatabaseDataQueryHandler>(@"Can Read From Database", failureStatus: HealthStatus.Unhealthy, tags: [HealthCheckConstants.ReadyTag]);

            services.AddSingleton<HealthCheckPublisher>();
            services.AddHostedService<DynamicHealthCheckPublisherService>();

            return services;
        }
    }
}
