namespace IdentityServer.Modules.Accounts.Responses
{
    /// <summary>Represents an IdentityServer account.</summary>
    /// <param name="Id">The identity user identifier.</param>
    /// <param name="Username">The local username.</param>
    /// <param name="Email">The account email address.</param>
    /// <param name="EmailConfirmed">Whether the email address has been confirmed.</param>
    public sealed record AccountResponse(
        Guid Id,
        string Username,
        string Email,
        bool EmailConfirmed);

    /// <summary>Represents a newly registered account and its confirmation-email delivery state.</summary>
    /// <param name="Id">The identity user identifier.</param>
    /// <param name="Username">The local username.</param>
    /// <param name="Email">The account email address.</param>
    /// <param name="EmailConfirmed">Whether the email address has already been confirmed.</param>
    /// <param name="ConfirmationEmailQueued">Whether a protected confirmation-email dispatch was staged successfully.</param>
    public sealed record AccountRegistrationResponse(
        Guid Id,
        string Username,
        string Email,
        bool EmailConfirmed,
        bool ConfirmationEmailQueued);

    /// <summary>Represents a security event recorded for an IdentityServer account.</summary>
    /// <param name="Id">The event identifier.</param>
    /// <param name="Action">The stable security action name.</param>
    /// <param name="ClientId">The related OpenID client identifier, when available.</param>
    /// <param name="IpAddress">The originating IP address, when available.</param>
    /// <param name="UserAgent">The originating user-agent value, when available.</param>
    /// <param name="DetailsJson">Optional structured JSON details.</param>
    /// <param name="OccurredUtc">The UTC event time.</param>
    public sealed record SecurityEventResponse(
        long Id,
        string Action,
        string? ClientId,
        string? IpAddress,
        string? UserAgent,
        string? DetailsJson,
        DateTime OccurredUtc);
}
