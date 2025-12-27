using Common.Cqrs;
using Common.Models.Response;

namespace Application.Accounts.Commands
{
    public class UpdateAccountCommand : ICommand<BasicResponse>
    {
        public required Guid Id { get; set; }
        public string? Username { get; set; } = null;
        public string? Email { get; set; } = null;
        public string? Password { get; set; } = null;
        public int? TokenVersion { get; set; } = null;
        public string[]? Roles { get; set; } = null;
        public byte? FailedLoginAttempts { get; set; } = null;
        public DateTime? LockedUntilUtc { get; set; } = null;
    }
}
