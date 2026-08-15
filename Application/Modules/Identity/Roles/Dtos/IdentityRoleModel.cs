namespace Application.Modules.Identity.Roles.Dtos
{
    public sealed record IdentityRoleModel
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}
