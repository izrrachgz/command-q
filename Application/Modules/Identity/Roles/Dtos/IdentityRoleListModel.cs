namespace Application.Modules.Identity.Roles.Dtos
{
    public sealed record IdentityRoleListModel
    {
        public IReadOnlyList<IdentityRoleModel> Roles { get; init; } = [];
    }
}
