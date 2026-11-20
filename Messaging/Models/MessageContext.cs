namespace Messaging.Models
{
    /// <summary>
    /// Transport-neutral metadata for the current message delivery.
    /// </summary>
    public sealed class MessageContext
    {
        public MessageContext(
            Guid messageId,
            string? correlationId,
            string? causationId,
            DateTimeOffset sentAtUtc,
            int deliveryAttempt,
            IReadOnlyDictionary<string, string> headers)
        {
            ArgumentNullException.ThrowIfNull(headers);

            if (messageId == Guid.Empty)
            {
                throw new ArgumentException("Message ID cannot be empty.", nameof(messageId));
            }

            if (deliveryAttempt < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(deliveryAttempt), deliveryAttempt, "Delivery attempt must be at least one.");
            }

            MessageId = messageId;
            CorrelationId = correlationId;
            CausationId = causationId;
            SentAtUtc = sentAtUtc.ToUniversalTime();
            DeliveryAttempt = deliveryAttempt;
            Headers = headers;
        }

        public Guid MessageId { get; }

        public string? CorrelationId { get; }

        public string? CausationId { get; }

        public DateTimeOffset SentAtUtc { get; }

        public int DeliveryAttempt { get; }

        public IReadOnlyDictionary<string, string> Headers { get; }
    }
}
