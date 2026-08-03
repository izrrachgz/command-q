using Email.Constants;

namespace Email.Configuration
{
    /// <summary>
    /// Sets pre-delivery safety limits for messages sent through this component.
    /// </summary>
    public sealed class EmailDeliveryPolicyOptions
    {
        public int MaxRecipients { get; set; } = EmailConstants.DefaultMaxRecipients;
        public int MaxAttachmentCount { get; set; } = EmailConstants.DefaultMaxAttachmentCount;
        public long MaxAttachmentBytes { get; set; } = EmailConstants.DefaultMaxAttachmentBytes;
        public long MaxTotalAttachmentBytes { get; set; } = EmailConstants.DefaultMaxTotalAttachmentBytes;
        public string[] AllowedSenderDomains { get; set; } = Array.Empty<string>();
    }
}
