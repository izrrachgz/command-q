using Common.Cqrs;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.HealthChecks.Queries
{
    public class GetDatabaseDataQuery : IQuery<HealthCheckResult>
    {
    }
}
