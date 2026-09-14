using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Modules.Accounts.Requests
{
    /// <summary>Requests delivery of an account confirmation or password recovery email.</summary>
    public sealed record RequestAccountEmailRequest
    {
        /// <summary>Gets the account email address without implying whether an account exists.</summary>
        [Required, EmailAddress]
        public string Email { get; init; } = string.Empty;

        /// <summary>Gets the preferred culture for the requested email.</summary>
        [StringLength(AccountConstants.MaxCultureLength)]
        public string? Culture { get; init; }
    }

    /// <summary>Contains the one-time link token and six-digit verification code for an account challenge.</summary>
    public sealed record VerifyAccountChallengeRequest
    {
        /// <summary>Gets the random one-time secret supplied by the application link.</summary>
        [Required]
        public string LinkToken { get; init; } = string.Empty;

        /// <summary>Gets the separate six-digit code supplied in the email body.</summary>
        [Required]
        public string Code { get; init; } = string.Empty;
    }

    /// <summary>Contains a verified recovery grant and the replacement password.</summary>
    public sealed record ResetPasswordRequest
    {
        /// <summary>Gets the short-lived grant issued after successful recovery challenge verification.</summary>
        [Required]
        public string ResetGrant { get; init; } = string.Empty;

        /// <summary>Gets the replacement password evaluated against the configured Identity password policy.</summary>
        [Required]
        public string NewPassword { get; init; } = string.Empty;
    }
}
