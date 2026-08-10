namespace Email.Outbox.Models
{
    public sealed record EmailOutboxBatchResult(int ClaimedCount, int SentCount, int RetryCount, int DeadLetterCount, int ReleasedCount)
    {
        public static EmailOutboxBatchResult Empty { get; } = new(0, 0, 0, 0, 0);
    }
}
