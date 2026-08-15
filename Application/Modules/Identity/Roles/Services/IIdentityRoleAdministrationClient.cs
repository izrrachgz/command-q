using Common.Models.Request;
using Common.Models.Response;
using Application.Modules.Identity.Roles.Dtos;

namespace Application.Modules.Identity.Roles.Services
{
    public interface IIdentityRoleAdministrationClient
    {
        Task<PagedResponse<IdentityUserRoleModel>> SearchUsers(SearchCriteria searchCriteria, CancellationToken cancellationToken);

        Task<DetailResponse<IdentityUserRoleModel>> GetUser(Guid userId, CancellationToken cancellationToken);

        Task<DetailResponse<IdentityRoleListModel>> GetRoles(CancellationToken cancellationToken);

        Task<BasicResponse> UpdateUserRoles(Guid userId, IReadOnlyList<string> roles, CancellationToken cancellationToken);
    }
}
