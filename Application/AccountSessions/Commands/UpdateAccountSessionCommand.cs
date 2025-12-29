using Common.Cqrs;
using Common.Models.Response;

namespace Application.AccountSessions.Commands
{
    public class UpdateAccountSessionCommand : ICommand<BasicResponse>
    {
        public required Guid AccountId { get; set; }
        public int? TokenVersion { get; set; } = null;
        public byte? FailedLoginAttempts { get; set; } = null;
        public DateTime? LockedUntilUtc { get; set; } = null;
    }
}
