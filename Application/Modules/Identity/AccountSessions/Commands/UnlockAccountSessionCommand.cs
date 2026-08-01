using Common.Cqrs;
using Common.Models.Response;

namespace Application.Modules.Identity.AccountSessions.Commands
{
    public class UnlockAccountSessionCommand : ICommand<BasicResponse>
    {
        public required Guid AccountId { get; set; }
    }
}
