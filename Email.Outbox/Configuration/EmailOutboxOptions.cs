using Email.Outbox.Constants;

namespace Email.Outbox.Configuration
{
    public sealed class EmailOutboxOptions
    {
        public int MaxAttempts { get; set; } = EmailOutboxConstants.DefaultMaxAttempts;
        public int BaseRetryDelaySeconds { get; set; } = EmailOutboxConstants.DefaultBaseRetryDelaySeconds;
        public int MaxRetryDelaySeconds { get; set; } = EmailOutboxConstants.DefaultMaxRetryDelaySeconds;
        public int MaxErrorLength { get; set; } = EmailOutboxConstants.DefaultMaxErrorLength;
        public long MaxPayloadBytes { get; set; } = EmailOutboxConstants.DefaultMaxPayloadBytes;
    }
}
