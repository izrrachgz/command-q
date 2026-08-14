using Email.Constants;

namespace Email.Configuration
{
    /// <summary>
    /// Sets pre-delivery safety limits for messages sent through this component.
    /// </summary>
    public sealed class EmailDeliveryPolicyOptions
    {
        /// <summary>
        /// Gets or sets the maximum number of recipients across To, Cc and Bcc.
        /// </summary>
        public int MaxRecipients { get; set; } = EmailConstants.DefaultMaxRecipients;

        /// <summary>
        /// Gets or sets the maximum number of attachments and inline resources.
        /// </summary>
        public int MaxAttachmentCount { get; set; } = EmailConstants.DefaultMaxAttachmentCount;

        /// <summary>
        /// Gets or sets the maximum raw byte size of one attachment or inline resource.
        /// </summary>
        public long MaxAttachmentBytes { get; set; } = EmailConstants.DefaultMaxAttachmentBytes;

        /// <summary>
        /// Gets or sets the maximum combined raw byte size of all attachments and inline resources.
        /// </summary>
        public long MaxTotalAttachmentBytes { get; set; } = EmailConstants.DefaultMaxTotalAttachmentBytes;

        /// <summary>
        /// Gets or sets the maximum serialized MIME message size sent to SMTP.
        /// </summary>
        public long MaxMessageBytes { get; set; } = EmailConstants.DefaultMaxMessageBytes;

        /// <summary>
        /// Gets or sets the allowed MIME content-type patterns for attachments and inline resources.
        /// </summary>
        public string[] AllowedAttachmentContentTypes { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the allowed file extensions for attachments and inline resources.
        /// </summary>
        public string[] AllowedAttachmentExtensions { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets whether content with no known declared or inferred MIME type is allowed.
        /// </summary>
        public bool AllowUnknownContentTypes { get; set; }

        /// <summary>
        /// Gets or sets the sender domains permitted by the profile.
        /// </summary>
        public string[] AllowedSenderDomains { get; set; } = Array.Empty<string>();
    }
}
