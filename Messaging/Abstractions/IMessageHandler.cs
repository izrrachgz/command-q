using Messaging.Models;

namespace Messaging.Abstractions
{
    /// <summary>
    /// Handles an asynchronously delivered message.
    /// </summary>
    /// <typeparam name="TMessage">The concrete message type.</typeparam>
    public interface IMessageHandler<in TMessage> where TMessage : class
    {
        /// <summary>
        /// Handles one delivery of the message.
        /// </summary>
        /// <param name="message">The deserialized message.</param>
        /// <param name="context">Transport-neutral delivery metadata.</param>
        /// <param name="cancellationToken">Cancellation requested by the worker host.</param>
        Task HandleAsync(TMessage message, MessageContext context, CancellationToken cancellationToken);
    }
}
