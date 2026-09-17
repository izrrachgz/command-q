using Email.Client.Models;

namespace Email.Client.Abstractions;

/// <summary>Queues and reads account messages through the command-emails protobuf API.</summary>
public interface IEmailQueueClient
{
    /// <summary>Durably queues one prepared email message.</summary>
    /// <param name="request">The message and delivery metadata to enqueue.</param>
    /// <param name="cancellationToken">Cancels the remote operation.</param>
    /// <returns>The durable queue result.</returns>
    Task<EmailQueueResult> EnqueueAsync(EmailQueueRequest request, CancellationToken cancellationToken = default);

    /// <summary>Gets one newest-first page of queued messages for an account reference.</summary>
    /// <param name="request">The account reference and paging values.</param>
    /// <param name="cancellationToken">Cancels the remote operation.</param>
    /// <returns>The requested page and its totals.</returns>
    Task<EmailQueuePageResult> GetPageAsync(EmailQueuePageRequest request, CancellationToken cancellationToken = default);

    /// <summary>Searches queued messages through the EmailAdmin-only API.</summary>
    Task<EmailQueuePageResult> SearchAsync(EmailQueueSearchRequest request, CancellationToken cancellationToken = default);

    /// <summary>Gets one complete EmailAdmin-visible queued message without attachment bytes.</summary>
    Task<QueuedEmailDetail?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
}
