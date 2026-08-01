using Common.Cqrs;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.Modules.Health.Queries
{
    public class CheckInternetConnectionQuery : IQuery<HealthCheckResult>
    {
    }
}
