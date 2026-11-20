using Messaging.Abstractions;
using Messaging.Models;
using Wolverine;

namespace Messaging.Wolverine.RabbitMq
{
    internal sealed class WolverineMessageSender(IMessageBus messageBus) : IMessageSender
    {
        public async ValueTask SendAsync<TMessage>(
            TMessage message,
            MessageSendOptions? options = null,
            CancellationToken cancellationToken = default)
            where TMessage : class
        {
            ArgumentNullException.ThrowIfNull(message);
            cancellationToken.ThrowIfCancellationRequested();

            await messageBus.SendAsync(message, createDeliveryOptions(options)).AsTask().WaitAsync(cancellationToken);
        }

        public async ValueTask PublishAsync<TMessage>(
            TMessage message,
            MessageSendOptions? options = null,
            CancellationToken cancellationToken = default)
            where TMessage : class
        {
            ArgumentNullException.ThrowIfNull(message);
            cancellationToken.ThrowIfCancellationRequested();

            await messageBus.PublishAsync(message, createDeliveryOptions(options)).AsTask().WaitAsync(cancellationToken);
        }

        public async ValueTask ScheduleAsync<TMessage>(
            TMessage message,
            DateTimeOffset deliverAtUtc,
            MessageSendOptions? options = null,
            CancellationToken cancellationToken = default)
            where TMessage : class
        {
            ArgumentNullException.ThrowIfNull(message);
            cancellationToken.ThrowIfCancellationRequested();

            DeliveryOptions deliveryOptions = createDeliveryOptions(options);
            deliveryOptions.ScheduledTime = deliverAtUtc.ToUniversalTime();
            await messageBus.SendAsync(message, deliveryOptions).AsTask().WaitAsync(cancellationToken);
        }

        private static DeliveryOptions createDeliveryOptions(MessageSendOptions? options)
        {
            if (options is null)
            {
                return new DeliveryOptions();
            }

            ArgumentNullException.ThrowIfNull(options.Headers);

            var deliveryOptions = new DeliveryOptions
            {
                DeliverBy = options.DeliverByUtc?.ToUniversalTime(),
                CorrelationId = normalize(options.CorrelationId),
                CausationId = normalize(options.CausationId),
                DeduplicationId = normalize(options.DeduplicationId),
                GroupId = normalize(options.GroupId),
                PartitionKey = normalize(options.PartitionKey)
            };

            foreach ((string key, string value) in options.Headers)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new ArgumentException("Message header names cannot be empty.", nameof(options));
                }

                ArgumentNullException.ThrowIfNull(value);
                deliveryOptions.WithHeader(key, value);
            }

            return deliveryOptions;
        }

        private static string? normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
