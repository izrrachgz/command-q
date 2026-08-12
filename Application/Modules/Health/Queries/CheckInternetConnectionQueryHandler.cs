using Common.Cqrs;
using Common.Extensions;
using Common.Transfer.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using Application.Modules.Health.Logging;
using Application.Modules.Health.Configuration;

namespace Application.Modules.Health.Queries
{
    internal sealed class CheckInternetConnectionQueryHandler : IQueryHandler<CheckInternetConnectionQuery, HealthCheckResult>, IHealthCheck
    {
        private readonly IOptionsMonitor<HealthCheckConfigurationOptions> _options;
        private readonly ILogger<CheckInternetConnectionQueryHandler> _logger;
        private readonly string UnhealthyMessage = @"Application is unable to access internet.";
        private readonly string HealthyMessage = @"Application is able to access internet.";

        public CheckInternetConnectionQueryHandler(
            IOptionsMonitor<HealthCheckConfigurationOptions> options,
            ILogger<CheckInternetConnectionQueryHandler> logger)
        {
            _logger = logger;
            _options = options;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return await Handle(new CheckInternetConnectionQuery(), cancellationToken);
        }

        public async Task<HealthCheckResult> Handle(CheckInternetConnectionQuery request, CancellationToken cancellationToken)
        {
            string urlCheckInternetAccess = _options.CurrentValue.UrlCheckInternetAccess;

            if (string.IsNullOrEmpty(urlCheckInternetAccess) || !urlCheckInternetAccess.IsUri())
                return HealthCheckResult.Unhealthy(UnhealthyMessage);

            var httpCall = new HttpCall();

            var response = await httpCall.Get(urlCheckInternetAccess, cancellationToken: cancellationToken);

            bool success = response.IsSuccessStatusCode;

            if (!success)
            {
                _logger.DependencyCheckFailed("internet connection");

                return HealthCheckResult.Unhealthy(UnhealthyMessage);
            }

            _logger.DependencyCheckSucceeded("internet connection");

            return HealthCheckResult.Healthy(HealthyMessage);
        }
    }
}
