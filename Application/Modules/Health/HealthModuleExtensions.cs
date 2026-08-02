using Application.Modules.Health.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

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
