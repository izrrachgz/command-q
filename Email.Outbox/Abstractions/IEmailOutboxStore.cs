using NodaTime;
using Email.Outbox.Models;

namespace Email.Outbox.Abstractions
{
    public interface IEmailOutboxStore
    {
        Task<EmailOutboxEnqueueResult> EnqueueAsync(EmailOutboxRecord record, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ClaimedEmailOutboxRecord>> ClaimPendingAsync(
            EmailOutboxClaimRequest request,
            CancellationToken cancellationToken = default);

        Task<bool> MarkSentAsync(EmailOutboxSentUpdate update, CancellationToken cancellationToken = default);

        Task<bool> MarkFailedAsync(EmailOutboxFailedUpdate update, CancellationToken cancellationToken = default);

        Task<int> ReleaseAsync(
            IReadOnlyCollection<ClaimedEmailOutboxRecord> records,
            Instant nextAttemptUtc,
            CancellationToken cancellationToken = default);
    }
}
