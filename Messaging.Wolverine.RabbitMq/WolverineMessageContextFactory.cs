using Messaging.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using Wolverine;

namespace Messaging.Wolverine.RabbitMq
{
    internal static class WolverineMessageContextFactory
    {
        public static MessageContext Create(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            Envelope envelope = serviceProvider.GetRequiredService<IMessageContext>().Envelope
                ?? throw new InvalidOperationException("Message context is only available while handling a Wolverine message.");

            Dictionary<string, string> headerValues = envelope.HasHeaders
                ? envelope.Headers
                    .Where(header => header.Value is not null)
                    .ToDictionary(header => header.Key, header => header.Value!, StringComparer.Ordinal)
                : new Dictionary<string, string>(StringComparer.Ordinal);
            var headers = new ReadOnlyDictionary<string, string>(headerValues);
            int deliveryAttempt = envelope.BrokerDeliveryCount ?? Math.Max(1, envelope.Attempts + 1);

            return new MessageContext(
                envelope.Id,
                envelope.CorrelationId,
                envelope.ParentId,
                envelope.SentAt,
                deliveryAttempt,
                headers);
        }
    }
}
