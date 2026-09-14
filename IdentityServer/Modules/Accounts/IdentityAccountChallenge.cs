namespace IdentityServer.Modules.Accounts
{
    /// <summary>Persists a hashed, expiring, one-time confirmation or password-recovery challenge for an identity user.</summary>
    internal sealed class IdentityAccountChallenge
    {
        /// <summary>Gets or sets the challenge identifier.</summary>
        public Guid Id { get; set; }
        /// <summary>Gets or sets the owning identity user identifier.</summary>
        public Guid UserId { get; set; }
        /// <summary>Gets or sets the workflow purpose that prevents cross-workflow use.</summary>
        public required string Purpose { get; set; }
        /// <summary>Gets or sets the keyed hash of the secret carried by the application link.</summary>
        public required string LinkTokenHash { get; set; }
        /// <summary>Gets or sets the keyed hash of the separately delivered six-digit code.</summary>
        public required string CodeHash { get; set; }
        /// <summary>Gets or sets the optimistic-concurrency token.</summary>
        public required string ConcurrencyStamp { get; set; }
        /// <summary>Gets or sets the number of failed link-and-code verification attempts.</summary>
        public int VerificationFailedAttempts { get; set; }
        /// <summary>Gets or sets the number of failed new-password submissions after verification.</summary>
        public int PasswordResetFailedAttempts { get; set; }
        /// <summary>Gets or sets the UTC creation time.</summary>
        public DateTime CreatedUtc { get; set; }
        /// <summary>Gets or sets the UTC expiration time.</summary>
        public DateTime ExpiresUtc { get; set; }
        /// <summary>Gets or sets when link-and-code verification succeeded.</summary>
        public DateTime? VerifiedUtc { get; set; }
        /// <summary>Gets or sets when the challenge was used or invalidated.</summary>
        public DateTime? ConsumedUtc { get; set; }

        /// <summary>Gets or sets the owning identity user navigation.</summary>
        public IdentityUser User { get; set; } = null!;
    }
}
