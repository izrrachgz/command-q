using Application.Modules.Health;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Api.Modules.Health
{
    /// <summary>
    /// Provides API health endpoint registration.
    /// </summary>
    public static class HealthApiModuleExtensions
    {
        /// <summary>
        /// Maps API health endpoints for liveness, readiness and full health reports.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder to configure.</param>
        /// <returns>The configured endpoint route builder.</returns>
        public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
        {
            ArgumentNullException.ThrowIfNull(endpoints);

            endpoints.MapHealthChecks(HealthRoute.Live, new HealthCheckOptions
            {
                Predicate = healthCheck => healthCheck.Tags.Contains(HealthCheckConstants.LiveTag)
            });

            endpoints.MapHealthChecks(HealthRoute.Ready, new HealthCheckOptions
            {
                Predicate = healthCheck => healthCheck.Tags.Contains(HealthCheckConstants.ReadyTag)
            });

            endpoints.MapHealthChecks(HealthRoute.Checks);

            return endpoints;
        }
    }
}
