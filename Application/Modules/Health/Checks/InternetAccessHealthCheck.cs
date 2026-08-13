using Common.Transfer.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Modules.Health.Logging;
using Application.Modules.Health.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.Modules.Health.Checks
{
    /// <summary>
    /// Verifies that the configured external URL can be reached.
    /// </summary>
    internal sealed class InternetAccessHealthCheck(
        IOptionsMonitor<HealthCheckConfigurationOptions> options,
        ILogger<InternetAccessHealthCheck> logger) : IHealthCheck
    {
        private const string HealthyMessage = "Application is able to access internet.";
        private const string UnhealthyMessage = "Application is unable to access internet.";

        /// <inheritdoc />
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return await CheckAsync(context.Registration.FailureStatus, cancellationToken);
        }

        /// <summary>
        /// Runs the internet-access probe using the default unhealthy failure status.
        /// </summary>
        /// <param name="cancellationToken">The token used to cancel the probe.</param>
        /// <returns>The resulting health-check status.</returns>
        internal async Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken = default)
        {
            return await CheckAsync(HealthStatus.Unhealthy, cancellationToken);
        }

        private async Task<HealthCheckResult> CheckAsync(HealthStatus failureStatus, CancellationToken cancellationToken)
        {
            string url = options.CurrentValue.UrlCheckInternetAccess;

            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                logger.DependencyCheckFailed("internet connection");

                return new HealthCheckResult(failureStatus, UnhealthyMessage);
            }

            var httpCall = new HttpCall();
            using HttpResponseMessage response = await httpCall.Get(url, cancellationToken: cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.DependencyCheckFailed("internet connection");

                return new HealthCheckResult(failureStatus, UnhealthyMessage);
            }

            logger.DependencyCheckSucceeded("internet connection");

            return HealthCheckResult.Healthy(HealthyMessage);
        }
    }
}
