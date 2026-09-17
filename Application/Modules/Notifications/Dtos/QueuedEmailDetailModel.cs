namespace Application.Modules.Notifications.Dtos;

/// <summary>Represents one administrator-visible queued email detail.</summary>
public sealed record QueuedEmailDetailModel
{
    public required Guid Id { get; init; }
    public required EmailMessageModel Message { get; init; }
    public required string ProfileName { get; init; }
    public string? IdempotencyKey { get; init; }
    public string? AccountReference { get; init; }
    public string? CorrelationId { get; init; }
    public required string Status { get; init; }
    public required int AttemptCount { get; init; }
    public required DateTime CreatedUtc { get; init; }
    public required DateTime ModifiedUtc { get; init; }
    public required DateTime NextAttemptUtc { get; init; }
    public DateTime? SentUtc { get; init; }
    public DateTime? DeadLetterUtc { get; init; }
    public string? LastFailureKind { get; init; }
    public string? MessageId { get; init; }
}
