using Cqrs.Abstractions;
using Common.Models.Response;
using Application.Modules.Identity.Roles.Services;

namespace Application.Modules.Identity.Roles.Commands
{
    internal sealed class UpdateIdentityUserRolesCommandHandler(IIdentityRoleAdministrationClient client) : ICommandHandler<UpdateIdentityUserRolesCommand, BasicResponse>
    {
        public Task<BasicResponse> Handle(
            UpdateIdentityUserRolesCommand request,
            CancellationToken cancellationToken)
        {
            return client.UpdateUserRoles(request.UserId, request.Roles, cancellationToken);
        }
    }
}
