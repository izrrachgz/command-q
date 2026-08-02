using IdentityServer.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IdentityServer.Modules.Health
{
    internal static class HealthModuleExtensions
    {
        public static IServiceCollection AddHealthModule(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck(
                    HealthCheckConstants.SelfName,
                    () => HealthCheckResult.Healthy(),
                    [HealthCheckConstants.LiveTag])
                .AddDbContextCheck<IdentityServerDbContext>(
                    HealthCheckConstants.DatabaseName,
                    tags: [HealthCheckConstants.ReadyTag]);

            return services;
        }

        public static IEndpointRouteBuilder MapHealthModule(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHealthChecks(HealthRoutes.Live, new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains(HealthCheckConstants.LiveTag)
            });

            endpoints.MapHealthChecks(HealthRoutes.Ready, new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains(HealthCheckConstants.ReadyTag)
            });

            endpoints.MapHealthChecks(HealthRoutes.Checks);

            return endpoints;
        }
    }
}
