using Messaging.Abstractions;
using Messaging.Models;
using Messaging.Wolverine.RabbitMq.Configuration;
using Messaging.Wolverine.RabbitMq.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Messaging.Wolverine.RabbitMq.Test
{
    public sealed class RabbitMqMessagingExtensionsTest
    {
        [Fact]
        public void Registration_Should_Add_Transport_Neutral_Services_And_Explicit_Handler()
        {
            var options = new WolverineOptions();
            IServiceCollection services = options.Services;

            RabbitMqMessagingBuilder builder = options.UseCommandQMessagingWithRabbitMq("amqp://guest:guest@localhost:5672/");
            builder.AddQueueWorker<TestMessage, TestMessageHandler>("test-work", worker => worker.MaxDegreeOfParallelism = 2);

            Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IMessageSender));
            Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(MessageContext));
            Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(TestMessageHandler));
            Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IMessageHandler<TestMessage>));
            Assert.True(options.Tracking.EnableMessageCausationTracking);
        }

        [Fact]
        public void Registration_Should_Reject_An_Empty_Connection_String()
        {
            var options = new WolverineOptions();

            Assert.Throws<ArgumentException>(() => options.UseCommandQMessagingWithRabbitMq(" "));
        }

        [Fact]
        public void Worker_Should_Reject_Invalid_Concurrency()
        {
            var options = new WolverineOptions();
            RabbitMqMessagingBuilder builder = options.UseCommandQMessagingWithRabbitMq("amqp://guest:guest@localhost:5672/");

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                builder.AddQueueWorker<TestMessage, TestMessageHandler>("test-work", worker => worker.MaxDegreeOfParallelism = 0));
        }

        [Fact]
        public void Worker_Should_Reject_Duplicate_Queue_Registrations()
        {
            var options = new WolverineOptions();
            RabbitMqMessagingBuilder builder = options.UseCommandQMessagingWithRabbitMq("amqp://guest:guest@localhost:5672/");
            builder.AddQueueWorker<TestMessage, TestMessageHandler>("test-work");

            Assert.Throws<InvalidOperationException>(() => builder.AddQueueWorker<TestMessage, TestMessageHandler>("test-work"));
        }

        [Fact]
        public void Worker_Options_Should_Require_An_Explicit_Single_Active_Consumer_Choice()
        {
            var options = new RabbitMqWorkerOptions();

            Assert.False(options.SingleActiveConsumer);
        }

        private sealed record TestMessage(string Value);

        private sealed class TestMessageHandler : IMessageHandler<TestMessage>
        {
            public Task HandleAsync(TestMessage message, MessageContext context, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
