using Application.Modules.Health.Queries;
using Application.Modules.Health.Commands;
using Application.Modules.Health.Publishers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.Modules.Health.Extensions
{
    public static class HealthCheckServiceExtensions
    {
        private const int SecondsDelayToRun = 1;
        private const int DaysToRunHealthCheck = 1;

        public static IServiceCollection ConfigureAppHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection healthCheckSection = configuration.GetSection(
                HealthCheckConstants.ConfigurationSection);

            services.AddOptions<HealthCheckConfigurationOptions>()
                .Bind(healthCheckSection)
                .Validate(options =>
                    Uri.TryCreate(options.UrlCheckInternetAccess, UriKind.Absolute, out _),
                    "HealthCheck:UrlCheckInternetAccess must be an absolute URI.")
                .ValidateOnStart();

            services.AddHealthChecks()
                .AddCheck(HealthCheckConstants.ApplicationRunningName, () => HealthCheckResult.Healthy(HealthCheckConstants.ApplicationRunningMessage), tags: [HealthCheckConstants.LiveTag])
                .AddTypeActivatedCheck<SaveLocalStorageDataCommandHandler>(@"Can Write On Local Storage", failureStatus: HealthStatus.Unhealthy, tags: [HealthCheckConstants.ReadyTag])
                .AddTypeActivatedCheck<CheckInternetConnectionQueryHandler>(@"Can Access Internet", failureStatus: HealthStatus.Unhealthy, tags: [HealthCheckConstants.ReadyTag])
                .AddTypeActivatedCheck<SaveDatabaseDataCommandHandler>(@"Can Write On Database", failureStatus: HealthStatus.Unhealthy, tags: [HealthCheckConstants.ReadyTag])
                .AddTypeActivatedCheck<GetDatabaseDataQueryHandler>(@"Can Read From Database", failureStatus: HealthStatus.Unhealthy, tags: [HealthCheckConstants.ReadyTag]);

            services.Configure<HealthCheckPublisherOptions>(options =>
            {
                options.Delay = TimeSpan.FromSeconds(SecondsDelayToRun);
                options.Period = TimeSpan.FromDays(DaysToRunHealthCheck);
                options.Predicate = healthCheck => healthCheck.Name.Contains(@"Can");
            });

            if (healthCheckSection.GetValue(nameof(HealthCheckConfigurationOptions.PublishOnStartup), false))
            {
                services.AddSingleton<IHealthCheckPublisher, HealthCheckPublisher>();
            }

            return services;
        }
    }
}
