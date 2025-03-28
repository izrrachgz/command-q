using Common.Cqrs;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.HealthChecks.Queries
{
    public class CheckInternetConnectionQuery : IQuery<HealthCheckResult>
    {
    }
}
