using Cqrs.Abstractions;
using Common.Models.Response;
using Application.Modules.Identity.Roles.Dtos;
using Application.Modules.Identity.Roles.Services;

namespace Application.Modules.Identity.Roles.Queries
{
    internal sealed class GetIdentityUserRolesQueryHandler(IIdentityRoleAdministrationClient client) : IQueryHandler<GetIdentityUserRolesQuery, DetailResponse<IdentityUserRoleModel>>
    {
        public Task<DetailResponse<IdentityUserRoleModel>> Handle(
            GetIdentityUserRolesQuery request,
            CancellationToken cancellationToken)
        {
            return client.GetUser(request.UserId, cancellationToken);
        }
    }
}
