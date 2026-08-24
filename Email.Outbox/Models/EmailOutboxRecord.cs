using NodaTime;
using Email.Outbox.Constants;

namespace Email.Outbox.Models
{
    /// <summary>
    /// Represents the serialized message and delivery metadata stored in the outbox.
    /// </summary>
    /// <param name="Id">The durable outbox record identifier.</param>
    /// <param name="Payload">The serialized email message payload.</param>
    /// <param name="IdempotencyKey">The optional idempotency key.</param>
    /// <param name="AccountReference">The optional application account reference.</param>
    /// <param name="CorrelationId">The optional caller-provided correlation reference.</param>
    /// <param name="AttemptCount">The number of delivery attempts already made.</param>
    /// <param name="CreatedUtc">The UTC timestamp at which the record was created.</param>
    /// <param name="NextAttemptUtc">The earliest UTC timestamp at which the record may be claimed.</param>
    /// <param name="ProfileName">The logical SMTP profile selected for delivery.</param>
    /// <param name="TraceParent">The optional W3C trace-parent value captured at enqueue time.</param>
    /// <param name="TraceState">The optional W3C trace-state value captured at enqueue time.</param>
    public sealed record EmailOutboxRecord(
        Guid Id,
        string Payload,
        string? IdempotencyKey,
        string? AccountReference,
        string? CorrelationId,
        int AttemptCount,
        Instant CreatedUtc,
        Instant NextAttemptUtc,
        string ProfileName = EmailOutboxConstants.DefaultProfileName,
        string? TraceParent = null,
        string? TraceState = null);
}
