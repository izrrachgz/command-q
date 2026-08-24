using NodaTime;

namespace Application.Modules.Notifications.Models
{
    /// <summary>
    /// Reports the durable outbox identity and UTC creation instant returned to an application workflow.
    /// </summary>
    public sealed record QueuedEmailResult(Guid Id, bool WasDuplicate, Instant CreatedUtc);
}
