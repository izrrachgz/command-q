using Email.Client.Models;

namespace Email.Client.Abstractions;

/// <summary>Queues messages through the command-emails protobuf API.</summary>
public interface IEmailQueueClient
{
    Task<EmailQueueResult> EnqueueAsync(EmailQueueRequest request, CancellationToken cancellationToken = default);
}
