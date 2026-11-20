using Messaging.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Wolverine;

namespace Messaging.Wolverine.RabbitMq.Test
{
    public sealed class WolverineMessageContextFactoryTest
    {
        [Fact]
        public void Create_Should_Map_Envelope_Metadata()
        {
            Guid messageId = Guid.NewGuid();
            var sentAt = new DateTimeOffset(2026, 11, 16, 8, 30, 0, TimeSpan.FromHours(-7));
            var envelope = new Envelope
            {
                Id = messageId,
                CorrelationId = "correlation",
                ParentId = "causation",
                SentAt = sentAt,
                BrokerDeliveryCount = 3
            };
            envelope.Headers["schema-version"] = "2";
            IMessageContext wolverineContext = createContext(envelope);
            using ServiceProvider services = new ServiceCollection()
                .AddSingleton(wolverineContext)
                .BuildServiceProvider();

            MessageContext context = WolverineMessageContextFactory.Create(services);

            Assert.Equal(messageId, context.MessageId);
            Assert.Equal("correlation", context.CorrelationId);
            Assert.Equal("causation", context.CausationId);
            Assert.Equal(3, context.DeliveryAttempt);
            Assert.Equal(TimeSpan.Zero, context.SentAtUtc.Offset);
            Assert.Equal("2", context.Headers["schema-version"]);
        }

        private static IMessageContext createContext(Envelope envelope)
        {
            IMessageContext context = DispatchProxy.Create<IMessageContext, MessageContextProxy>();
            ((MessageContextProxy)context).Envelope = envelope;
            return context;
        }

        private class MessageContextProxy : DispatchProxy
        {
            public Envelope? Envelope { get; set; }

            protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            {
                ArgumentNullException.ThrowIfNull(targetMethod);

                return targetMethod.Name == "get_Envelope" ? Envelope : null;
            }
        }
    }
}
