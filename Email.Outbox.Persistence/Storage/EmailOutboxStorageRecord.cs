using NodaTime;
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
        public Instant CreatedUtc { get; set; }
        public Instant ModifiedUtc { get; set; }
        public Instant NextAttemptUtc { get; set; }
        public Instant? LockedUntilUtc { get; set; }
        public string? LockId { get; set; }
        public Instant? SentUtc { get; set; }
        public Instant? DeadLetterUtc { get; set; }
        public int? LastFailureKind { get; set; }
        public string? LastError { get; set; }
        public string? MessageId { get; set; }
        public string? SmtpResponse { get; set; }
    }
}
