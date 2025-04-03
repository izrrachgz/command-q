#nullable disable
using Application.Constants;
using Common.Cqrs;
using Common.Extensions;
using Common.Transfer.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.HealthChecks.Queries
{
    public class CheckInternetConnectionQueryHandler : IQueryHandler<CheckInternetConnectionQuery, HealthCheckResult>, IHealthCheck
    {
        private readonly IConfiguration _configuration;
        private readonly string UnhealthyMessage = @"Application is unable to access internet.";
        private readonly string HealthyMessage = @"Application is able to access internet.";

        public CheckInternetConnectionQueryHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return await Handle(new CheckInternetConnectionQuery(), cancellationToken);
        }

        public async Task<HealthCheckResult> Handle(CheckInternetConnectionQuery request, CancellationToken cancellationToken)
        {
            string urlCheckInternetAccess = _configuration.GetValue<string>("HealthCheck:UrlCheckInternetAccess");

            if (!urlCheckInternetAccess.IsUri())
                return HealthCheckResult.Unhealthy(UnhealthyMessage);

            var httpCall = new HttpCall();

            var response = await httpCall.Get(urlCheckInternetAccess, cancellationToken: cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return HealthCheckResult.Unhealthy(ApplicationMessages.QueryCancelled);
            }

            bool success = response.IsSuccessStatusCode;

            if (!success)
                return HealthCheckResult.Unhealthy(UnhealthyMessage);

            return HealthCheckResult.Healthy(HealthyMessage);
        }
    }
}
