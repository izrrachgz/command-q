using Email.Configuration;
using Email.Outbox.Constants;

namespace Email.Outbox.Configuration
{
    /// <summary>
    /// Configures durable email queue processing and producer-side content limits.
    /// </summary>
    public sealed class EmailOutboxOptions
    {
        /// <summary>
        /// Gets or sets the producer-side content policy applied before serialization.
        /// </summary>
        public EmailDeliveryPolicyOptions DeliveryPolicy { get; set; } = new();

        /// <summary>
        /// Gets or sets the maximum number of delivery attempts.
        /// </summary>
        public int MaxAttempts { get; set; } = EmailOutboxConstants.DefaultMaxAttempts;

        /// <summary>
        /// Gets or sets the base retry delay in seconds.
        /// </summary>
        public int BaseRetryDelaySeconds { get; set; } = EmailOutboxConstants.DefaultBaseRetryDelaySeconds;

        /// <summary>
        /// Gets or sets the maximum retry delay in seconds.
        /// </summary>
        public int MaxRetryDelaySeconds { get; set; } = EmailOutboxConstants.DefaultMaxRetryDelaySeconds;

        /// <summary>
        /// Gets or sets the maximum persisted error length.
        /// </summary>
        public int MaxErrorLength { get; set; } = EmailOutboxConstants.DefaultMaxErrorLength;

        /// <summary>
        /// Gets or sets the maximum serialized JSON payload size in bytes.
        /// </summary>
        public long MaxPayloadBytes { get; set; } = EmailOutboxConstants.DefaultMaxPayloadBytes;
    }
}
