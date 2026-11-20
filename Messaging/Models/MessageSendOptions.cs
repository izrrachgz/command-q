namespace Messaging.Models
{
    /// <summary>
    /// Optional transport-neutral metadata and delivery constraints for an outgoing message.
    /// </summary>
    public sealed class MessageSendOptions
    {
        public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();

        public DateTimeOffset? DeliverByUtc { get; init; }

        public string? CorrelationId { get; init; }

        public string? CausationId { get; init; }

        public string? DeduplicationId { get; init; }

        public string? GroupId { get; init; }

        public string? PartitionKey { get; init; }
    }
}
