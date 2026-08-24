using NodaTime;

namespace Email.Outbox.Models
{
    /// <summary>
    /// Defines the state transition for a message accepted by the SMTP server.
    /// </summary>
    public sealed record EmailOutboxSentUpdate(
        Guid Id,
        string LockId,
        int AttemptCount,
        string MessageId,
        string SmtpResponse,
        Instant SentUtc);
}
