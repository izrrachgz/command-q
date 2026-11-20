namespace Messaging.Wolverine.RabbitMq.Configuration
{
    /// <summary>
    /// Application-wide RabbitMQ transport defaults.
    /// </summary>
    public sealed class RabbitMqMessagingOptions
    {
        public bool AutoProvision { get; set; }

        public bool UseQuorumQueues { get; set; } = true;

        public bool EnablePublisherConfirmations { get; set; } = true;

        public bool UseStaticHandlerRegistry { get; set; } = true;

        public TimeSpan BrokerInitializationTimeout { get; set; } = TimeSpan.FromMinutes(2);
    }
}
