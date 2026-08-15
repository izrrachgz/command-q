using Common.Cqrs;
using Common.Models.Response;
using Application.Modules.Identity.Roles.Dtos;
using Application.Modules.Identity.Roles.Services;

namespace Application.Modules.Identity.Roles.Queries
{
    internal sealed class GetIdentityRolesQueryHandler(IIdentityRoleAdministrationClient client) : IQueryHandler<GetIdentityRolesQuery, DetailResponse<IdentityRoleListModel>>
    {
        public Task<DetailResponse<IdentityRoleListModel>> Handle(
            GetIdentityRolesQuery request,
            CancellationToken cancellationToken)
        {
            return client.GetRoles(cancellationToken);
        }
    }
}
