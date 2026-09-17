namespace Application.Modules.Notifications.Dtos;

/// <summary>Represents one account email and its current delivery state.</summary>
public sealed record QueuedEmailModel
{
    /// <summary>Gets the durable queue entry identifier.</summary>
    public required Guid Id { get; init; }

    public required string Subject { get; init; }

    public required IReadOnlyList<string> Recipients { get; init; }

    public string? AccountReference { get; init; }

    /// <summary>Gets the current delivery status.</summary>
    public required string Status { get; init; }

    /// <summary>Gets the number of delivery attempts made so far.</summary>
    public required int AttemptCount { get; init; }

    /// <summary>Gets when the queue entry was created, in UTC.</summary>
    public required DateTime CreatedUtc { get; init; }

    /// <summary>Gets when the queue entry was last modified, in UTC.</summary>
    public required DateTime ModifiedUtc { get; init; }

    /// <summary>Gets when delivery completed, in UTC.</summary>
    public DateTime? SentUtc { get; init; }

    /// <summary>Gets when the entry entered the dead-letter state, in UTC.</summary>
    public DateTime? DeadLetterUtc { get; init; }

    /// <summary>Gets the provider message identifier, when available.</summary>
    public string? MessageId { get; init; }
}
