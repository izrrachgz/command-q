namespace Email.Outbox.Models
{
    public sealed record EmailOutboxSentUpdate(
        Guid Id,
        string LockId,
        int AttemptCount,
        string MessageId,
        string SmtpResponse,
        DateTime SentUtc);
}
