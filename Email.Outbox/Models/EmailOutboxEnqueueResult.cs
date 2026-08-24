using NodaTime;

namespace Email.Outbox.Models
{
    /// <summary>
    /// Reports the durable identity and creation instant of an enqueue operation.
    /// </summary>
    /// <param name="Id">The durable outbox identifier.</param>
    /// <param name="WasDuplicate">Whether an existing idempotency record satisfied the request.</param>
    /// <param name="CreatedUtc">The unambiguous UTC creation instant.</param>
    public sealed record EmailOutboxEnqueueResult(Guid Id, bool WasDuplicate, Instant CreatedUtc);
}
