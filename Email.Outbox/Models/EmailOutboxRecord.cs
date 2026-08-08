namespace Email.Outbox.Models
{
    public sealed record EmailOutboxRecord(
        Guid Id,
        string Payload,
        string? IdempotencyKey,
        string? AccountReference,
        string? CorrelationId,
        int AttemptCount,
        DateTime CreatedUtc,
        DateTime NextAttemptUtc);
}
