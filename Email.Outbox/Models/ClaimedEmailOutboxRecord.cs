using NodaTime;
using Email.Outbox.Constants;

namespace Email.Outbox.Models
{
    /// <summary>
    /// Represents an email outbox record acquired by a worker lease.
    /// </summary>
    /// <param name="Id">The durable outbox record identifier.</param>
    /// <param name="Payload">The serialized email message payload.</param>
    /// <param name="LockId">The lease identifier required for state updates.</param>
    /// <param name="AttemptCount">The number of delivery attempts already made.</param>
    /// <param name="CorrelationId">The optional caller-provided correlation reference.</param>
    /// <param name="CreatedUtc">The UTC timestamp used to measure queue age.</param>
    /// <param name="ProfileName">The logical SMTP profile selected for delivery.</param>
    /// <param name="TraceParent">The optional W3C trace-parent value captured at enqueue time.</param>
    /// <param name="TraceState">The optional W3C trace-state value captured at enqueue time.</param>
    public sealed record ClaimedEmailOutboxRecord(
        Guid Id,
        string Payload,
        string LockId,
        int AttemptCount,
        string? CorrelationId,
        Instant CreatedUtc,
        string ProfileName = EmailOutboxConstants.DefaultProfileName,
        string? TraceParent = null,
        string? TraceState = null);
}
