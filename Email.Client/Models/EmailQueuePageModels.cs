namespace Email.Client.Models;

/// <summary>Delivery state reported by the Email API for a durable queue entry.</summary>
public enum EmailDeliveryStatus
{
    Unspecified = 0,
    Pending = 1,
    Processing = 2,
    Sent = 3,
    DeadLetter = 4
}

/// <summary>Selects one newest-first page of messages for an account reference.</summary>
public sealed record EmailQueuePageRequest
{
    /// <summary>Gets the service-defined account reference.</summary>
    public required string AccountReference { get; init; }

    /// <summary>Gets the one-based page number.</summary>
    public int Page { get; init; } = 1;

    /// <summary>Gets the maximum number of entries in the page.</summary>
    public int PageSize { get; init; } = EmailQueueConstants.DefaultPageSize;
}

/// <summary>Selects one EmailAdmin-only page across queued messages.</summary>
public sealed record EmailQueueSearchRequest
{
    public string? AccountReference { get; init; }
    public string? SearchTerm { get; init; }
    public string? Recipient { get; init; }
    public EmailDeliveryStatus? Status { get; init; }
    public DateTime? StartUtc { get; init; }
    public DateTime? EndUtc { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = EmailQueueConstants.DefaultPageSize;
}

/// <summary>Represents one lightweight queued-email search result.</summary>
public sealed record QueuedEmailSummary
{
    /// <summary>Gets the durable queue entry identifier.</summary>
    public required Guid Id { get; init; }

    public required string Subject { get; init; }

    public required IReadOnlyList<string> Recipients { get; init; }

    /// <summary>Gets the account reference associated with the entry.</summary>
    public string? AccountReference { get; init; }

    /// <summary>Gets the current delivery state.</summary>
    public required EmailDeliveryStatus Status { get; init; }

    /// <summary>Gets the number of delivery attempts made so far.</summary>
    public required int AttemptCount { get; init; }

    /// <summary>Gets when the entry was created, in UTC.</summary>
    public required DateTime CreatedUtc { get; init; }

    /// <summary>Gets when the entry was last modified, in UTC.</summary>
    public required DateTime ModifiedUtc { get; init; }

    /// <summary>Gets when delivery completed, in UTC.</summary>
    public DateTime? SentUtc { get; init; }

    /// <summary>Gets when the entry entered the dead-letter state, in UTC.</summary>
    public DateTime? DeadLetterUtc { get; init; }

    /// <summary>Gets the last categorized failure, when available.</summary>
    public string? LastFailureKind { get; init; }

    /// <summary>Gets the provider message identifier, when available.</summary>
    public string? MessageId { get; init; }
}

public sealed record EmailContentMetadata(
    string FileName,
    string? ContentType,
    long Size,
    string? ContentId = null);

public sealed record EmailMessageDetail
{
    public EmailAddress? From { get; init; }
    public required IReadOnlyList<EmailAddress> To { get; init; }
    public required IReadOnlyList<EmailAddress> Cc { get; init; }
    public required IReadOnlyList<EmailAddress> Bcc { get; init; }
    public required IReadOnlyList<EmailAddress> ReplyTo { get; init; }
    public required string Subject { get; init; }
    public string? TextBody { get; init; }
    public string? HtmlBody { get; init; }
    public required IReadOnlyList<EmailContentMetadata> Attachments { get; init; }
    public required IReadOnlyList<EmailContentMetadata> InlineResources { get; init; }
}

public sealed record QueuedEmailDetail
{
    public required Guid Id { get; init; }
    public required EmailMessageDetail Message { get; init; }
    public required string ProfileName { get; init; }
    public string? IdempotencyKey { get; init; }
    public string? AccountReference { get; init; }
    public string? CorrelationId { get; init; }
    public required EmailDeliveryStatus Status { get; init; }
    public required int AttemptCount { get; init; }
    public required DateTime CreatedUtc { get; init; }
    public required DateTime ModifiedUtc { get; init; }
    public required DateTime NextAttemptUtc { get; init; }
    public DateTime? SentUtc { get; init; }
    public DateTime? DeadLetterUtc { get; init; }
    public string? LastFailureKind { get; init; }
    public string? MessageId { get; init; }
}

/// <summary>Contains one page of queued account messages and aggregate paging values.</summary>
public sealed record EmailQueuePageResult
{
    /// <summary>Gets the entries in this page.</summary>
    public required IReadOnlyList<QueuedEmailSummary> Items { get; init; }

    /// <summary>Gets the one-based page number.</summary>
    public required int Page { get; init; }

    /// <summary>Gets the requested page size.</summary>
    public required int PageSize { get; init; }

    /// <summary>Gets the total number of matching entries.</summary>
    public required long TotalCount { get; init; }

    /// <summary>Gets the total number of matching pages.</summary>
    public required long TotalPages { get; init; }
}
