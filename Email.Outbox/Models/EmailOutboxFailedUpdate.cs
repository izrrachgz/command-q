using NodaTime;

namespace Email.Outbox.Models
{
    /// <summary>
    /// Defines a retry or dead-letter transition at an explicit UTC instant.
    /// </summary>
    public sealed record EmailOutboxFailedUpdate(
        Guid Id,
        string LockId,
        int AttemptCount,
        EmailOutboxFailureKind FailureKind,
        string Error,
        Instant NextAttemptUtc,
        bool IsDeadLetter,
        Instant FailedUtc);
}
