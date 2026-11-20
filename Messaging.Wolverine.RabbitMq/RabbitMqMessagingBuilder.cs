using Messaging.Abstractions;
using Messaging.Wolverine.RabbitMq.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;
using Wolverine;
using Wolverine.RabbitMQ;
using Wolverine.RabbitMQ.Internal;

namespace Messaging.Wolverine.RabbitMq
{
    /// <summary>
    /// Explicitly registers message routes and competing-consumer workers.
    /// </summary>
    public sealed class RabbitMqMessagingBuilder
    {
        private const string SingleActiveConsumerArgument = "x-single-active-consumer";
        private readonly HashSet<(Type MessageType, string QueueName)> _routes = [];
        private readonly HashSet<string> _singleActiveConsumerQueues = new(StringComparer.Ordinal);
        private readonly HashSet<string> _workerQueues = new(StringComparer.Ordinal);
        private readonly RabbitMqTransportExpression _transport;
        private readonly bool _useQuorumQueues;
        private readonly WolverineOptions _wolverineOptions;

        internal RabbitMqMessagingBuilder(
            WolverineOptions wolverineOptions,
            RabbitMqTransportExpression transport,
            bool useQuorumQueues)
        {
            _wolverineOptions = wolverineOptions;
            _transport = transport;
            _useQuorumQueues = useQuorumQueues;
        }

        /// <summary>
        /// Routes a message type to one RabbitMQ work queue without adding a local consumer.
        /// </summary>
        public RabbitMqMessagingBuilder RouteToQueue<TMessage>(string queueName) where TMessage : class
        {
            validateQueueName(queueName);

            if (_routes.Add((typeof(TMessage), queueName)))
            {
                _wolverineOptions
                    .PublishMessage<TMessage>()
                    .ToRabbitQueue(queueName, queue => configureQueue(queue, queueName));
            }

            return this;
        }

        /// <summary>
        /// Registers a handler and a competing-consumer listener for a RabbitMQ work queue.
        /// </summary>
        public RabbitMqMessagingBuilder AddQueueWorker<
            TMessage,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>(
            string queueName,
            Action<RabbitMqWorkerOptions>? configure = null)
            where TMessage : class
            where THandler : class, IMessageHandler<TMessage>
        {
            validateQueueName(queueName);

            var workerOptions = new RabbitMqWorkerOptions();
            configure?.Invoke(workerOptions);
            validate(workerOptions);

            if (!_workerQueues.Add(queueName))
            {
                throw new InvalidOperationException($"Queue '{queueName}' already has a worker registration in this host.");
            }

            if (workerOptions.SingleActiveConsumer)
            {
                _singleActiveConsumerQueues.Add(queueName);
            }

            RouteToQueue<TMessage>(queueName);
            _wolverineOptions.Services.TryAddTransient<THandler>();
            _wolverineOptions.Services.TryAddEnumerable(ServiceDescriptor.Transient<IMessageHandler<TMessage>, THandler>());
            _wolverineOptions.Discovery.IncludeType<THandler>();

            _wolverineOptions
                .ListenToRabbitQueue(queueName, queue => configureQueue(queue, queueName))
                .ProcessInParallelWithNativeAcks()
                .PreFetchCount(workerOptions.PrefetchCount)
                .MaximumParallelMessages(workerOptions.MaxDegreeOfParallelism);

            return this;
        }

        private void configureQueue(IRabbitMqQueue queue, string queueName)
        {
            queue.IsDurable = true;
            queue.AutoDelete = false;

            if (_useQuorumQueues)
            {
                queue.QueueType = QueueType.quorum;
            }

            if (_singleActiveConsumerQueues.Contains(queueName))
            {
                queue.Arguments[SingleActiveConsumerArgument] = true;
            }
        }

        private static void validate(RabbitMqWorkerOptions options)
        {
            if (options.PrefetchCount == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options), options.PrefetchCount, "Prefetch count must be greater than zero.");
            }

            if (options.MaxDegreeOfParallelism <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(options),
                    options.MaxDegreeOfParallelism,
                    "Maximum degree of parallelism must be greater than zero.");
            }
        }

        private static void validateQueueName(string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentException("Queue name cannot be empty.", nameof(queueName));
            }
        }
    }
}
