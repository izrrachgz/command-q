using Email.Outbox.Models;

namespace Email.Outbox.Abstractions
{
    public interface IEmailOutboxProcessor
    {
        Task<EmailOutboxBatchResult> ProcessBatchAsync(
            EmailOutboxClaimRequest request,
            CancellationToken cancellationToken = default);
    }
}
