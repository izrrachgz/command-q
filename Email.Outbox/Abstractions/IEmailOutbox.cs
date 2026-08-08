using Email.Models;
using Email.Outbox.Models;

namespace Email.Outbox.Abstractions
{
    public interface IEmailOutbox
    {
        Task<EmailOutboxEnqueueResult> EnqueueAsync(
            EmailMessage message,
            CancellationToken cancellationToken = default);

        Task<EmailOutboxEnqueueResult> EnqueueAsync(
            EmailOutboxRequest request,
            CancellationToken cancellationToken = default);
    }
}
