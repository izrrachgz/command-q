using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Modules.Accounts
{
    public sealed record IdentityAdminSearchCriteria
    {
        public string? SearchTerm { get; init; }
        public int Page { get; init; } = 1;
        public int Size { get; init; } = 25;
        public string? SortBy { get; init; }
        public bool AscendingSort { get; init; } = true;
    }

    public sealed record IdentityAdminRoleResponse(Guid Id, string Name);

    public sealed record IdentityAdminUserResponse(
        Guid Id,
        string Username,
        string Email,
        bool Active,
        bool Locked,
        IReadOnlyList<string> Roles,
        DateTime CreatedUtc,
        DateTimeOffset? LockedUntilUtc);

    public sealed record IdentityAdminPagedResponse<T>(
        IReadOnlyList<T> Collection,
        long Total,
        int Page,
        int PageSize,
        int TotalPages);

    public sealed record UpdateIdentityUserRolesRequest
    {
        [Required]
        public IReadOnlyList<string> Roles { get; init; } = [];
    }
}
