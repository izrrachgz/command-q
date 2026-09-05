using Cqrs.Abstractions;
using Common.Models.Response;
using Application.Abstractions;

namespace Application.Modules.Identity.Accounts.Commands
{
    public class CreateAccountCommand : ICommand<BasicResponse>, ISystemScopedRequest
    {
        public required string Email { get; set; }
        public required string Username { get; set; }
        public required string IdentityIssuer { get; set; }
        public required string IdentitySubject { get; set; }
    }
}
