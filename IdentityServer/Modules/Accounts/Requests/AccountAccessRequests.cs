using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Modules.Accounts.Requests
{
    /// <summary>Contains the credentials and profile values required to register a local account.</summary>
    public sealed record RegisterAccountRequest
    {
        /// <summary>Gets the unique local username.</summary>
        [Required]
        public string Username { get; init; } = string.Empty;

        /// <summary>Gets the unique email address that must be confirmed before local sign-in.</summary>
        [Required, EmailAddress]
        public string Email { get; init; } = string.Empty;

        /// <summary>Gets the initial password evaluated against the configured Identity password policy.</summary>
        [Required]
        public string Password { get; init; } = string.Empty;

        /// <summary>Gets the preferred culture for the confirmation email.</summary>
        [StringLength(AccountConstants.MaxCultureLength)]
        public string? Culture { get; init; }
    }

    /// <summary>Contains the credentials used to sign in to a local account.</summary>
    public sealed record SignInRequest
    {
        /// <summary>Gets the local username or email address used to locate the account.</summary>
        [Required]
        public string UsernameOrEmail { get; init; } = string.Empty;

        /// <summary>Gets the local account password.</summary>
        [Required]
        public string Password { get; init; } = string.Empty;

        /// <summary>Gets whether the authentication cookie should persist beyond the browser session.</summary>
        public bool RememberMe { get; init; }
    }
}
