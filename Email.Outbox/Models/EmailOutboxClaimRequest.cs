namespace Email.Outbox.Models
{
    public sealed record EmailOutboxClaimRequest(int BatchSize, string WorkerId, TimeSpan LeaseDuration, DateTime UtcNow);
}
