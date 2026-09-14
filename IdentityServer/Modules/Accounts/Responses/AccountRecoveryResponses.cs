namespace IdentityServer.Modules.Accounts.Responses
{
    /// <summary>Returns the short-lived grant that activates the password reset form.</summary>
    /// <param name="ResetGrant">The protected grant accepted by the password-reset endpoint.</param>
    /// <param name="ExpiresUtc">The UTC time at which the grant stops being valid.</param>
    public sealed record PasswordRecoveryGrantResponse(string ResetGrant, DateTime ExpiresUtc);
}
