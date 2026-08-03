namespace Email.Models
{
    /// <summary>
    /// Contains a complete email message ready for MIME construction and delivery.
    /// </summary>
    public sealed record EmailMessage
    {
        public EmailAddress? From { get; init; }
        public IReadOnlyCollection<EmailAddress> To { get; init; } = Array.Empty<EmailAddress>();
        public IReadOnlyCollection<EmailAddress> Cc { get; init; } = Array.Empty<EmailAddress>();
        public IReadOnlyCollection<EmailAddress> Bcc { get; init; } = Array.Empty<EmailAddress>();
        public IReadOnlyCollection<EmailAddress> ReplyTo { get; init; } = Array.Empty<EmailAddress>();
        public string Subject { get; init; } = string.Empty;
        public string? TextBody { get; init; }
        public string? HtmlBody { get; init; }
        public IReadOnlyCollection<EmailAttachment> Attachments { get; init; } = Array.Empty<EmailAttachment>();
        public IReadOnlyCollection<EmailInlineResource> InlineResources { get; init; } = Array.Empty<EmailInlineResource>();
    }
}
