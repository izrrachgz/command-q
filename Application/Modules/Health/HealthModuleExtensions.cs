using Application.Modules.Health.Checks;
using Microsoft.Extensions.Configuration;
using Application.Modules.Health.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Modules.Health
{
    /// <summary>
    /// Provides dependency-injection registration for the Health module.
    /// </summary>
    public static class HealthModuleExtensions
    {
        /// <summary>
        /// Registers the Health module's operational health checks.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configuration">The host configuration used by health checks.</param>
        /// <returns>The configured service collection.</returns>
        public static IServiceCollection AddHealthModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureAppHealthChecks(configuration);

            services.AddTransient<DatabaseReadHealthCheck>();
            services.AddTransient<DatabaseWriteHealthCheck>();
            services.AddTransient<InternetAccessHealthCheck>();
            services.AddTransient<LocalStorageWriteHealthCheck>();

            return services;
        }
    }
}
