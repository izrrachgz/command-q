namespace IdentityServer.Modules.Accounts.Responses
{
    /// <summary>Represents an assignable IdentityServer role.</summary>
    /// <param name="Id">The role identifier.</param>
    /// <param name="Name">The role name.</param>
    public sealed record IdentityAdminRoleResponse(Guid Id, string Name);

    /// <summary>Represents an IdentityServer user in an administrative response.</summary>
    /// <param name="Id">The identity user identifier.</param>
    /// <param name="Username">The local username.</param>
    /// <param name="Email">The account email address.</param>
    /// <param name="EmailConfirmed">Whether the email address has been confirmed.</param>
    /// <param name="Active">Whether the account can currently authenticate.</param>
    /// <param name="Locked">Whether any account lock is currently active.</param>
    /// <param name="RequiresAdminUnlock">Whether only an administrator may unlock the account.</param>
    /// <param name="Roles">The roles currently assigned to the user.</param>
    /// <param name="CreatedUtc">The UTC account creation time.</param>
    /// <param name="AdminLockoutUtc">When the administrator-required lock was applied, when applicable.</param>
    /// <param name="LockedUntilUtc">When an ordinary temporary Identity lock expires, when applicable.</param>
    public sealed record IdentityAdminUserResponse(
        Guid Id,
        string Username,
        string Email,
        bool EmailConfirmed,
        bool Active,
        bool Locked,
        bool RequiresAdminUnlock,
        IReadOnlyList<string> Roles,
        DateTime CreatedUtc,
        DateTime? AdminLockoutUtc,
        DateTimeOffset? LockedUntilUtc);

    /// <summary>Represents one page of IdentityServer administration results.</summary>
    /// <typeparam name="T">The response item type.</typeparam>
    /// <param name="Collection">The items in the current page.</param>
    /// <param name="Total">The total number of matching items.</param>
    /// <param name="Page">The current one-based page number.</param>
    /// <param name="PageSize">The effective page size.</param>
    /// <param name="TotalPages">The total number of pages.</param>
    public sealed record IdentityAdminPagedResponse<T>(
        IReadOnlyList<T> Collection,
        long Total,
        int Page,
        int PageSize,
        int TotalPages);
}
