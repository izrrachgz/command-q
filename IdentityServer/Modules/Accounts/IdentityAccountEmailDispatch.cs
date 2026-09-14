namespace IdentityServer.Modules.Accounts
{
    /// <summary>Persists a protected account-email payload until it is accepted by the external Email API queue.</summary>
    internal sealed class IdentityAccountEmailDispatch
    {
        /// <summary>Gets or sets the dispatch identifier.</summary>
        public Guid Id { get; set; }
        /// <summary>Gets or sets the related account challenge identifier.</summary>
        public Guid ChallengeId { get; set; }
        /// <summary>Gets or sets the Data Protection-encrypted payload, cleared after queueing or abandonment.</summary>
        public string? ProtectedPayload { get; set; }
        /// <summary>Gets or sets the optimistic-concurrency token.</summary>
        public required string ConcurrencyStamp { get; set; }
        /// <summary>Gets or sets the number of failed Email API queue attempts.</summary>
        public int AttemptCount { get; set; }
        /// <summary>Gets or sets the UTC creation time.</summary>
        public DateTime CreatedUtc { get; set; }
        /// <summary>Gets or sets the earliest UTC time at which another queue attempt may run.</summary>
        public DateTime NextAttemptUtc { get; set; }
        /// <summary>Gets or sets when the Email API accepted the dispatch.</summary>
        public DateTime? QueuedUtc { get; set; }
        /// <summary>Gets or sets when the dispatch was discarded because it could no longer be delivered safely.</summary>
        public DateTime? AbandonedUtc { get; set; }

        /// <summary>Gets or sets the related account challenge navigation.</summary>
        public IdentityAccountChallenge Challenge { get; set; } = null!;
    }
}
