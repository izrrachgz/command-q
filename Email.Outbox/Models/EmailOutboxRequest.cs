using Email.Models;

namespace Email.Outbox.Models
{
    public sealed record EmailOutboxRequest
    {
        public required EmailMessage Message { get; init; }

        /// <summary>
        /// Gets the logical SMTP profile used by the worker, or <see langword="null"/> for the configured default.
        /// </summary>
        public string? ProfileName { get; init; }
        public string? IdempotencyKey { get; init; }
        public string? AccountReference { get; init; }
        public string? CorrelationId { get; init; }
    }
}
