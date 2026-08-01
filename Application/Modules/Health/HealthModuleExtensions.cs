using Application.Modules.Health.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Modules.Health
{
    public static class HealthModuleExtensions
    {
        public static IServiceCollection AddHealthModule(this IServiceCollection services)
        {
            services.ConfigureAppHealthChecks();

            return services;
        }
    }
}
