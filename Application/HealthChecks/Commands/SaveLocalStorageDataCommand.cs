using Common.Cqrs;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.HealthChecks.Commands
{
    public class SaveLocalStorageDataCommand : ICommand<HealthCheckResult>
    {
    }
}
