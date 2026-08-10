namespace Application.Modules.Notifications.Models
{
    public sealed record QueuedEmailResult(Guid Id, bool WasDuplicate, DateTime CreatedUtc);
}
