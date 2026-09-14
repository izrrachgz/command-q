using Application.Modules.Identity.Roles.Services;
using Common.Models.Response;
using Cqrs.Abstractions;

namespace Application.Modules.Identity.Roles.Commands
{
    internal sealed class UnlockIdentityUserCommandHandler(IIdentityRoleAdministrationClient client)
        : ICommandHandler<UnlockIdentityUserCommand, BasicResponse>
    {
        public Task<BasicResponse> Handle(UnlockIdentityUserCommand request, CancellationToken cancellationToken)
        {
            return client.UnlockUser(request.UserId, cancellationToken);
        }
    }
}
