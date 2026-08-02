using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Modules.Accounts
{
    public sealed record RegisterAccountRequest
    {
        [Required]
        public string Username { get; init; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; init; } = string.Empty;

        [Required]
        public string Password { get; init; } = string.Empty;
    }

    public sealed record SignInRequest
    {
        [Required]
        public string UsernameOrEmail { get; init; } = string.Empty;

        [Required]
        public string Password { get; init; } = string.Empty;

        public bool RememberMe { get; init; }
    }

    public sealed record AccountResponse(Guid Id, string Username, string Email);

    public sealed record SecurityEventResponse(
        long Id,
        string Action,
        string? ClientId,
        string? IpAddress,
        string? UserAgent,
        string? DetailsJson,
        DateTime OccurredUtc);
}
