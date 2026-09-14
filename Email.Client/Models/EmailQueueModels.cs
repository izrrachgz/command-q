namespace Email.Client.Models;

/// <summary>Represents an email mailbox and optional display name.</summary>
public sealed record EmailAddress(string Address, string? Name = null);

/// <summary>Represents binary email content loaded from a file or held in memory.</summary>
public sealed record EmailAttachment
{
    public EmailAttachment(string filePath, string? fileName = null, string? contentType = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        FilePath = filePath;
        FileName = string.IsNullOrWhiteSpace(fileName) ? Path.GetFileName(filePath) : fileName;
        ContentType = contentType;
    }

    private EmailAttachment(string fileName, ReadOnlyMemory<byte> content, string? contentType)
    {
        FileName = fileName;
        Content = content;
        ContentType = contentType;
    }

    public string? FilePath { get; }
    public string FileName { get; }
    public string? ContentType { get; }
    public ReadOnlyMemory<byte>? Content { get; }

    public static EmailAttachment FromBytes(string fileName, ReadOnlyMemory<byte> content, string? contentType = null) =>
        new(fileName, content.ToArray(), contentType);
}

/// <summary>Represents binary content referenced by a content ID from the HTML body.</summary>
public sealed record EmailInlineResource(string FileName, string ContentId, ReadOnlyMemory<byte> Content, string? ContentType = null);

/// <summary>Contains a complete email message ready to enqueue through the Email API.</summary>
public sealed record EmailMessage
{
    public EmailAddress? From { get; init; }
    public IReadOnlyCollection<EmailAddress> To { get; init; } = [];
    public IReadOnlyCollection<EmailAddress> Cc { get; init; } = [];
    public IReadOnlyCollection<EmailAddress> Bcc { get; init; } = [];
    public IReadOnlyCollection<EmailAddress> ReplyTo { get; init; } = [];
    public string Subject { get; init; } = string.Empty;
    public string? TextBody { get; init; }
    public string? HtmlBody { get; init; }
    public IReadOnlyCollection<EmailAttachment> Attachments { get; init; } = [];
    public IReadOnlyCollection<EmailInlineResource> InlineResources { get; init; } = [];
}

/// <summary>Represents one durable Email API enqueue operation.</summary>
public sealed record EmailQueueRequest
{
    public required EmailMessage Message { get; init; }
    public string? ProfileName { get; init; }
    public string? IdempotencyKey { get; init; }
    public string? AccountReference { get; init; }
    public string? CorrelationId { get; init; }
}

/// <summary>Reports the durable identity returned by the Email API.</summary>
public sealed record EmailQueueResult(Guid Id, bool WasDuplicate, DateTime CreatedUtc);
