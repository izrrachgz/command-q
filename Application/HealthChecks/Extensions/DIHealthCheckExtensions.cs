using Application.HealthChecks.Queries;
using Application.HealthChecks.Commands;
using Application.HealthChecks.Publishers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.HealthChecks.Extensions
{
    public static class DIHealthCheckExtensions
    {
        private const int SecondsDelayToRun = 1;
        private const int DaysToRunHealthCheck = 1;

        public static IServiceCollection ConfigureAppHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddTypeActivatedCheck<SaveLocalStorageDataCommandHandler>(@"Can Write On Local Storage", failureStatus: HealthStatus.Unhealthy)
                .AddTypeActivatedCheck<CheckInternetConnectionQueryHandler>(@"Can Access Internet", failureStatus: HealthStatus.Unhealthy)
                .AddTypeActivatedCheck<SaveDatabaseDataCommandHandler>(@"Can Write On Database", failureStatus: HealthStatus.Unhealthy)
                .AddTypeActivatedCheck<GetDatabaseDataQueryHandler>(@"Can Read From Database", failureStatus: HealthStatus.Unhealthy);

            services.Configure<HealthCheckPublisherOptions>(options =>
            {
                options.Delay = TimeSpan.FromSeconds(SecondsDelayToRun);
                options.Period = TimeSpan.FromDays(DaysToRunHealthCheck);
                options.Predicate = healthCheck => healthCheck.Name.Contains(@"Can");
            });

            services.AddSingleton<IHealthCheckPublisher, HealthCheckPublisher>();

            return services;
        }
    }
}
