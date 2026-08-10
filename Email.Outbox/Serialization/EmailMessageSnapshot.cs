namespace Email.Outbox.Serialization
{
    internal sealed record EmailMessageSnapshot(
        EmailAddressSnapshot? From,
        EmailAddressSnapshot[] To,
        EmailAddressSnapshot[] Cc,
        EmailAddressSnapshot[] Bcc,
        EmailAddressSnapshot[] ReplyTo,
        string Subject,
        string? TextBody,
        string? HtmlBody,
        EmailContentSnapshot[] Attachments,
        EmailInlineContentSnapshot[] InlineResources);

    internal sealed record EmailAddressSnapshot(string Address, string? Name);

    internal sealed record EmailContentSnapshot(string FileName, string? ContentType, byte[] Content);

    internal sealed record EmailInlineContentSnapshot(string FileName, string ContentId, string? ContentType, byte[] Content);
}
