namespace Email.Outbox.Models
{
    public sealed record EmailOutboxEnqueueResult(Guid Id, bool WasDuplicate, DateTime CreatedUtc);
}
