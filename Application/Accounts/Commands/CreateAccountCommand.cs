using Common.Cqrs;
using Common.Models.Response;

namespace Application.Accounts.Commands
{
    public class CreateAccountCommand : ICommand<BasicResponse>
    {
        public required string Email { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
