using Messaging.Models;

namespace Messaging.Abstractions
{
    /// <summary>
    /// Sends commands to one logical consumer and publishes events to all configured consumers.
    /// </summary>
    public interface IMessageSender
    {
        /// <summary>
        /// Sends a message and requires at least one configured destination.
        /// </summary>
        ValueTask SendAsync<TMessage>(
            TMessage message,
            MessageSendOptions? options = null,
            CancellationToken cancellationToken = default)
            where TMessage : class;

        /// <summary>
        /// Publishes a message to every configured destination and succeeds when no destination exists.
        /// </summary>
        ValueTask PublishAsync<TMessage>(
            TMessage message,
            MessageSendOptions? options = null,
            CancellationToken cancellationToken = default)
            where TMessage : class;

        /// <summary>
        /// Schedules a message for delivery at or after the supplied instant.
        /// </summary>
        ValueTask ScheduleAsync<TMessage>(
            TMessage message,
            DateTimeOffset deliverAtUtc,
            MessageSendOptions? options = null,
            CancellationToken cancellationToken = default)
            where TMessage : class;
    }
}
