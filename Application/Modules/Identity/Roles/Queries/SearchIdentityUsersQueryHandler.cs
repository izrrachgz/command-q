using Cqrs.Abstractions;
using Common.Models.Response;
using Application.Modules.Identity.Roles.Dtos;
using Application.Modules.Identity.Roles.Services;

namespace Application.Modules.Identity.Roles.Queries
{
    internal sealed class SearchIdentityUsersQueryHandler(IIdentityRoleAdministrationClient client) : IQueryHandler<SearchIdentityUsersQuery, PagedResponse<IdentityUserRoleModel>>
    {
        public Task<PagedResponse<IdentityUserRoleModel>> Handle(
            SearchIdentityUsersQuery request,
            CancellationToken cancellationToken)
        {
            return client.SearchUsers(request.SearchCriteria ?? new(), cancellationToken);
        }
    }
}
