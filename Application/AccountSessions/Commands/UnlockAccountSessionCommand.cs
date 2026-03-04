using Common.Cqrs;
using Common.Models.Response;

namespace Application.AccountSessions.Commands
{
    public class UnlockAccountSessionCommand : ICommand<BasicResponse>
    {
        public required Guid AccountId { get; set; }
    }
}
