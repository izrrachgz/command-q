using Email.Outbox.Persistence.Health;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Email.Worker.Modules.Health
{
    /// <summary>
    /// Registers and maps HTTP health endpoints owned by the email worker host.
    /// </summary>
    internal static class EmailWorkerHealthModuleExtensions
    {
        /// <summary>
        /// Adds liveness and readiness checks for the email worker host.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <returns>The configured service collection.</returns>
        public static IServiceCollection AddEmailWorkerHealthModule(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck(
                    EmailWorkerHealthConstants.SelfName,
                    () => HealthCheckResult.Healthy(EmailWorkerHealthConstants.RunningMessage),
                    tags: [EmailWorkerHealthConstants.LiveTag])
                .AddEmailOutboxPersistenceHealthCheck(
                    EmailWorkerHealthConstants.OutboxPersistenceName,
                    failureStatus: HealthStatus.Unhealthy,
                    tags: [EmailWorkerHealthConstants.ReadyTag]);

            return services;
        }

        /// <summary>
        /// Maps email worker health endpoints for liveness, readiness and full health reports.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder to configure.</param>
        /// <returns>The configured endpoint route builder.</returns>
        public static IEndpointRouteBuilder MapEmailWorkerHealthModule(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHealthChecks(EmailWorkerHealthRoutes.Live, new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains(EmailWorkerHealthConstants.LiveTag)
            });

            endpoints.MapHealthChecks(EmailWorkerHealthRoutes.Ready, new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains(EmailWorkerHealthConstants.ReadyTag)
            });

            endpoints.MapHealthChecks(EmailWorkerHealthRoutes.Checks);

            return endpoints;
        }
    }
}
