using Email.Outbox.Constants;

namespace Email.Outbox.Persistence.Storage
{
    internal sealed class EmailOutboxStorageRecord
    {
        public string Id { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public string ProfileName { get; set; } = EmailOutboxConstants.DefaultProfileName;
        public string? IdempotencyKey { get; set; }
        public string? AccountReference { get; set; }
        public string? CorrelationId { get; set; }
        public string? TraceParent { get; set; }
        public string? TraceState { get; set; }
        public int Status { get; set; }
        public int AttemptCount { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime ModifiedUtc { get; set; }
        public DateTime NextAttemptUtc { get; set; }
        public DateTime? LockedUntilUtc { get; set; }
        public string? LockId { get; set; }
        public DateTime? SentUtc { get; set; }
        public DateTime? DeadLetterUtc { get; set; }
        public int? LastFailureKind { get; set; }
        public string? LastError { get; set; }
        public string? MessageId { get; set; }
        public string? SmtpResponse { get; set; }
    }
}
