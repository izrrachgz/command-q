using Common.Models.Request;
using Common.Models.Response;
using Application.Modules.Identity.Roles.Dtos;

namespace Application.Modules.Identity.Roles.Services
{
    internal sealed class UnavailableIdentityRoleAdministrationClient : IIdentityRoleAdministrationClient
    {
        private const string NotConfiguredMessage = "Identity role administration client is not configured.";

        public Task<PagedResponse<IdentityUserRoleModel>> SearchUsers(
            SearchCriteria searchCriteria,
            CancellationToken cancellationToken)
        {
            var criteria = searchCriteria ?? new();

            return Task.FromResult(new PagedResponse<IdentityUserRoleModel>(
                [],
                total: 0,
                page: Math.Max(1, criteria.Page),
                itemsPerPage: Math.Max(1, criteria.Size)));
        }

        public Task<DetailResponse<IdentityUserRoleModel>> GetUser(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(new DetailResponse<IdentityUserRoleModel>(false, NotConfiguredMessage, null));
        }

        public Task<DetailResponse<IdentityRoleListModel>> GetRoles(CancellationToken cancellationToken)
        {
            return Task.FromResult(new DetailResponse<IdentityRoleListModel>(false, NotConfiguredMessage, null));
        }

        public Task<BasicResponse> UpdateUserRoles(
            Guid userId,
            IReadOnlyList<string> roles,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new BasicResponse(false, NotConfiguredMessage));
        }

        public Task<BasicResponse> UnlockUser(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(new BasicResponse(false, NotConfiguredMessage));
        }
    }
}
