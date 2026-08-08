namespace Email.Outbox.Models
{
    public sealed record EmailOutboxFailedUpdate(
        Guid Id,
        string LockId,
        int AttemptCount,
        EmailOutboxFailureKind FailureKind,
        string Error,
        DateTime NextAttemptUtc,
        bool IsDeadLetter,
        DateTime FailedUtc);
}
