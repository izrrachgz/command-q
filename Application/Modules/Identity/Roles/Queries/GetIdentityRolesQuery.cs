using Cqrs.Abstractions;
using Common.Models.Response;
using Application.Modules.Identity.Roles.Dtos;

namespace Application.Modules.Identity.Roles.Queries
{
    public sealed record GetIdentityRolesQuery : IQuery<DetailResponse<IdentityRoleListModel>>;
}
