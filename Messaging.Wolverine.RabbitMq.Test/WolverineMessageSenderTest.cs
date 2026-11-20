using Messaging.Models;
using System.Reflection;
using Wolverine;

namespace Messaging.Wolverine.RabbitMq.Test
{
    public sealed class WolverineMessageSenderTest
    {
        [Fact]
        public async Task SendAsync_Should_Map_Transport_Neutral_Options()
        {
            (IMessageBus bus, MessageBusProxy proxy) = createBus();
            var sender = new WolverineMessageSender(bus);
            var deliverBy = new DateTimeOffset(2026, 11, 16, 12, 0, 0, TimeSpan.FromHours(-7));
            var options = new MessageSendOptions
            {
                CorrelationId = "correlation",
                CausationId = "causation",
                DeduplicationId = "deduplication",
                GroupId = "group",
                PartitionKey = "partition",
                DeliverByUtc = deliverBy,
                Headers = new Dictionary<string, string> { ["schema-version"] = "1" }
            };

            await sender.SendAsync(new TestMessage("value"), options, TestContext.Current.CancellationToken);

            Assert.Equal(nameof(IMessageBus.SendAsync), proxy.LastMethodName);
            DeliveryOptions delivery = Assert.IsType<DeliveryOptions>(proxy.LastArguments![1]);
            Assert.Equal("correlation", delivery.CorrelationId);
            Assert.Equal("causation", delivery.CausationId);
            Assert.Equal("deduplication", delivery.DeduplicationId);
            Assert.Equal("group", delivery.GroupId);
            Assert.Equal("partition", delivery.PartitionKey);
            Assert.Equal(deliverBy.ToUniversalTime(), delivery.DeliverBy);
            Assert.Equal("1", delivery.Headers["schema-version"]);
        }

        [Fact]
        public async Task ScheduleAsync_Should_Use_An_Absolute_Utc_Time()
        {
            (IMessageBus bus, MessageBusProxy proxy) = createBus();
            var sender = new WolverineMessageSender(bus);
            var deliverAt = new DateTimeOffset(2026, 11, 17, 9, 0, 0, TimeSpan.FromHours(-7));

            await sender.ScheduleAsync(new TestMessage("value"), deliverAt, cancellationToken: TestContext.Current.CancellationToken);

            DeliveryOptions delivery = Assert.IsType<DeliveryOptions>(proxy.LastArguments![1]);
            Assert.Equal(deliverAt.ToUniversalTime(), delivery.ScheduledTime);
        }

        [Fact]
        public async Task SendAsync_Should_Propagate_Already_Requested_Cancellation()
        {
            (IMessageBus bus, MessageBusProxy proxy) = createBus();
            var sender = new WolverineMessageSender(bus);
            using var cancellationSource = new CancellationTokenSource();
            await cancellationSource.CancelAsync();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await sender.SendAsync(new TestMessage("value"), cancellationToken: cancellationSource.Token));

            Assert.Null(proxy.LastMethodName);
        }

        private static (IMessageBus Bus, MessageBusProxy Proxy) createBus()
        {
            IMessageBus bus = DispatchProxy.Create<IMessageBus, MessageBusProxy>();
            return (bus, (MessageBusProxy)bus);
        }

        private sealed record TestMessage(string Value);

        private class MessageBusProxy : DispatchProxy
        {
            public string? LastMethodName { get; private set; }

            public object?[]? LastArguments { get; private set; }

            protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            {
                ArgumentNullException.ThrowIfNull(targetMethod);

                if (targetMethod.Name.StartsWith("set_", StringComparison.Ordinal))
                {
                    return null;
                }

                if (targetMethod.Name.StartsWith("get_", StringComparison.Ordinal))
                {
                    return null;
                }

                LastMethodName = targetMethod.Name;
                LastArguments = args;
                return targetMethod.ReturnType == typeof(ValueTask) ? ValueTask.CompletedTask : null;
            }
        }
    }
}
