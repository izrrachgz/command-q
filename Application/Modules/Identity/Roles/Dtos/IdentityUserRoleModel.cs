namespace Application.Modules.Identity.Roles.Dtos
{
    public sealed record IdentityUserRoleModel
    {
        public Guid Id { get; init; }
        public string Username { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public bool Active { get; init; }
        public bool Locked { get; init; }
        public bool RequiresAdminUnlock { get; init; }
        public IReadOnlyList<string> Roles { get; init; } = [];
        public DateTime CreatedUtc { get; init; }
        public DateTime? AdminLockoutUtc { get; init; }
        public DateTimeOffset? LockedUntilUtc { get; init; }
    }
}
