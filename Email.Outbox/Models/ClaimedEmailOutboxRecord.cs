namespace Email.Outbox.Models
{
    public sealed record ClaimedEmailOutboxRecord(Guid Id, string Payload, string LockId, int AttemptCount, string? CorrelationId);
}
