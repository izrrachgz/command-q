namespace Email.Models
{
    /// <summary>
    /// Captures the accepted messages from a completed SMTP batch.
    /// </summary>
    public sealed record EmailBatchResult(IReadOnlyList<EmailSendResult> Messages)
    {
        public static EmailBatchResult Empty { get; } = new(Array.Empty<EmailSendResult>());
        public int SentCount => Messages.Count;
    }
}
