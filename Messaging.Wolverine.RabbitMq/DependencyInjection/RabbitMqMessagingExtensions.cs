using Messaging.Abstractions;
using Messaging.Wolverine.RabbitMq.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Wolverine;
using Wolverine.RabbitMQ;

namespace Messaging.Wolverine.RabbitMq.DependencyInjection
{
    public static class RabbitMqMessagingExtensions
    {
        /// <summary>
        /// Adds CommandQ messaging contracts backed by Wolverine and RabbitMQ.
        /// </summary>
        public static RabbitMqMessagingBuilder UseCommandQMessagingWithRabbitMq(
            this WolverineOptions wolverineOptions,
            string connectionString,
            Action<RabbitMqMessagingOptions>? configure = null)
        {
            ArgumentNullException.ThrowIfNull(wolverineOptions);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("RabbitMQ connection string cannot be empty.", nameof(connectionString));
            }

            var messagingOptions = new RabbitMqMessagingOptions();
            configure?.Invoke(messagingOptions);
            validate(messagingOptions);

            var transport = wolverineOptions.UseRabbitMq(connectionString);
            transport.ConfigureChannelCreation(channelOptions =>
            {
                channelOptions.PublisherConfirmationsEnabled = messagingOptions.EnablePublisherConfirmations;
                channelOptions.PublisherConfirmationTrackingEnabled = messagingOptions.EnablePublisherConfirmations;
            });

            if (messagingOptions.UseQuorumQueues)
            {
                transport.UseQuorumQueues();
            }

            if (messagingOptions.AutoProvision)
            {
                transport.AutoProvision();
            }

            if (messagingOptions.UseStaticHandlerRegistry)
            {
                wolverineOptions.UseStaticRegistries();
            }

            wolverineOptions.BrokerInitializationTimeout = messagingOptions.BrokerInitializationTimeout;
            wolverineOptions.Tracking.EnableMessageCausationTracking = true;
            wolverineOptions.Services.TryAddScoped<IMessageSender, WolverineMessageSender>();
            wolverineOptions.Services.TryAddScoped(WolverineMessageContextFactory.Create);

            return new RabbitMqMessagingBuilder(wolverineOptions, transport, messagingOptions.UseQuorumQueues);
        }

        private static void validate(RabbitMqMessagingOptions options)
        {
            if (options.BrokerInitializationTimeout <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(options),
                    options.BrokerInitializationTimeout,
                    "Broker initialization timeout must be greater than zero.");
            }
        }
    }
}
