using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.Accounts.Contracts.Recovery
{
    /// <summary>Describes the externally relevant result of account challenge verification.</summary>
    /// <param name="Succeeded">Whether verification succeeded.</param>
    /// <param name="LockedOut">Whether the account requires administrator intervention.</param>
    public sealed record AccountVerificationResult(bool Succeeded, bool LockedOut);

    /// <summary>Contains the Identity password-reset result and permanent-lockout state.</summary>
    /// <param name="Result">The ASP.NET Core Identity password-reset result.</param>
    /// <param name="LockedOut">Whether this attempt permanently locked the account.</param>
    public sealed record PasswordResetResult(IdentityResult Result, bool LockedOut);

    /// <summary>Contains a short-lived reset grant created after successful code verification.</summary>
    /// <param name="Succeeded">Whether link-token and code verification succeeded.</param>
    /// <param name="LockedOut">Whether the account requires administrator intervention.</param>
    /// <param name="ResetGrant">The protected reset grant when verification succeeded.</param>
    /// <param name="ExpiresUtc">The UTC expiration of the reset grant when verification succeeded.</param>
    public sealed record PasswordRecoveryVerificationResult(
        bool Succeeded,
        bool LockedOut,
        string? ResetGrant,
        DateTime? ExpiresUtc);
}
