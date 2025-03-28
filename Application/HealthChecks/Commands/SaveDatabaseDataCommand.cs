using Common.Cqrs;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.HealthChecks.Commands
{
    public class SaveDatabaseDataCommand : ICommand<HealthCheckResult>
    {
    }
}
