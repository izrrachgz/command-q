using Email.Outbox.Extensions;
using Email.Outbox.Abstractions;
using Email.Outbox.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Email.Outbox.Test.Extensions
{
    public sealed class EmailOutboxServiceCollectionExtensionsTest
    {
        [Fact]
        public void AddEmailOutboxProducer_Should_Not_Register_Delivery_Processor()
        {
            var services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder().Build();

            services.AddEmailOutboxProducer(configuration);

            Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IEmailOutbox));
            Assert.DoesNotContain(
                services,
                descriptor => descriptor.ServiceType == typeof(IEmailOutboxProcessor));
        }

        [Fact]
        public void AddEmailOutbox_Should_Register_Producer_And_Delivery_Processor()
        {
            var services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder().Build();

            services.AddEmailOutbox(configuration);

            Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IEmailOutbox));
            Assert.Contains(
                services,
                descriptor => descriptor.ServiceType == typeof(IEmailOutboxProcessor));
        }

        [Fact]
        public void AddEmailOutboxProducer_Should_Bind_An_Explicit_Host_Section()
        {
            var services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["TestHost:Queue:MaxAttempts"] = "9"
                })
                .Build();

            services.AddEmailOutboxProducer(
                configuration.GetRequiredSection("TestHost:Queue"));

            using var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<EmailOutboxOptions>>().Value;

            Assert.Equal(9, options.MaxAttempts);
        }
    }
}
