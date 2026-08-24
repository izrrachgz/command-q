using NodaTime;

namespace Email.Outbox.Models
{
    /// <summary>
    /// Defines a bounded outbox claim using an explicit UTC instant.
    /// </summary>
    /// <param name="BatchSize">The maximum number of records to claim.</param>
    /// <param name="WorkerId">The identifier of the claiming worker instance.</param>
    /// <param name="LeaseDuration">The duration for which the claim remains exclusive.</param>
    /// <param name="UtcNow">The UTC instant used for eligibility and lease calculations.</param>
    public sealed record EmailOutboxClaimRequest(int BatchSize, string WorkerId, Duration LeaseDuration, Instant UtcNow);
}
