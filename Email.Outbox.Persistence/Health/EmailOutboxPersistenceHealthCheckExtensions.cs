using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Email.Outbox.Persistence.Health
{
    /// <summary>
    /// Provides health-check registration helpers for email outbox persistence.
    /// </summary>
    public static class EmailOutboxPersistenceHealthCheckExtensions
    {
        /// <summary>
        /// Registers a health check that verifies the configured email outbox persistence store is reachable.
        /// </summary>
        /// <param name="builder">The health-check builder to configure.</param>
        /// <param name="name">The registration name for the email outbox persistence health check.</param>
        /// <param name="failureStatus">The status to report when the store cannot be reached.</param>
        /// <param name="tags">Optional tags used to group or filter the health check.</param>
        /// <returns>The configured health-check builder.</returns>
        public static IHealthChecksBuilder AddEmailOutboxPersistenceHealthCheck(
            this IHealthChecksBuilder builder,
            string name,
            HealthStatus? failureStatus = null,
            IEnumerable<string>? tags = null)
        {
            ArgumentNullException.ThrowIfNull(builder);

            return builder.Add(new HealthCheckRegistration(
                name,
                serviceProvider => ActivatorUtilities.CreateInstance<EmailOutboxPersistenceHealthCheck>(serviceProvider),
                failureStatus,
                tags));
        }
    }
}
