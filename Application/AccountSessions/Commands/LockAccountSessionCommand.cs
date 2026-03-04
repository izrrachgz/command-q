using Common.Cqrs;
using Common.Models.Response;

namespace Application.AccountSessions.Commands
{
    public class LockAccountSessionCommand : ICommand<BasicResponse>
    {
        public required Guid AccountId { get; set; }
        public required TimeSpan LockTime { get; set; } = TimeSpan.FromMinutes(15);
    }
}
