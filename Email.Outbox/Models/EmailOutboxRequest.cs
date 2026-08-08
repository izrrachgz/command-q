using Email.Models;

namespace Email.Outbox.Models
{
    public sealed record EmailOutboxRequest
    {
        public required EmailMessage Message { get; init; }
        public string? IdempotencyKey { get; init; }
        public string? AccountReference { get; init; }
        public string? CorrelationId { get; init; }
    }
}
