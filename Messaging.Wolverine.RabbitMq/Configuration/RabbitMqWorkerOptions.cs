namespace Messaging.Wolverine.RabbitMq.Configuration
{
    /// <summary>
    /// Throughput and concurrency settings for one competing-consumer work queue.
    /// </summary>
    public sealed class RabbitMqWorkerOptions
    {
        public ushort PrefetchCount { get; set; } = 16;

        public int MaxDegreeOfParallelism { get; set; } = 4;

        /// <summary>
        /// Limits consumption to one active consumer across every host instance attached to the queue.
        /// </summary>
        public bool SingleActiveConsumer { get; set; }
    }
}
