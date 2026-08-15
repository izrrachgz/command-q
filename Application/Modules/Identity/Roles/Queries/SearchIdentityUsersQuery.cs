using Common.Cqrs;
using Common.Models.Request;
using Common.Models.Response;
using Application.Modules.Identity.Roles.Dtos;

namespace Application.Modules.Identity.Roles.Queries
{
    public sealed record SearchIdentityUsersQuery : IQuery<PagedResponse<IdentityUserRoleModel>>
    {
        public SearchCriteria SearchCriteria { get; init; } = new();
    }
}
