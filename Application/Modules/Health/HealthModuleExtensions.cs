using Microsoft.Extensions.Configuration;
using Application.Modules.Health.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Modules.Health
{
    public static class HealthModuleExtensions
    {
        public static IServiceCollection AddHealthModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureAppHealthChecks(configuration);

            return services;
        }
    }
}
