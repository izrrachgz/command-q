using Common.Cqrs;
using Common.Models.Response;

namespace Application.Modules.Identity.Roles.Commands
{
    public sealed record UpdateIdentityUserRolesCommand : ICommand<BasicResponse>
    {
        public required Guid UserId { get; init; }
        public IReadOnlyList<string> Roles { get; init; } = [];
    }
}
