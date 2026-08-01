using Common.Cqrs;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.Modules.Health.Commands
{
    public class SaveDatabaseDataCommand : ICommand<HealthCheckResult>
    {
    }
}
