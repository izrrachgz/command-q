using Email.Extensions;
using Email.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Email.Test.Extensions
{
    public sealed class EmailServiceCollectionExtensionsTest
    {
        [Fact]
        public void AddEmailServices_Should_Load_Project_Defaults_Then_Apply_Host_Overrides()
        {
            var overrides = new Dictionary<string, string?>
            {
                ["Email:Enabled"] = "true",
                ["Email:Smtp:Host"] = "smtp.override.test",
                ["Email:Authentication:Type"] = "Plain",
                ["Email:Authentication:Plain:Username"] = "override-user",
                ["Email:Authentication:Plain:Password"] = "override-password",
                ["Email:Sender:Address"] = "sender@override.test"
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(overrides)
                .Build();
            var services = new ServiceCollection();

            services.AddEmailServices(configuration);

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<EmailOptions>>().Value;

            Assert.Equal("smtp.override.test", options.Smtp.Host);
            Assert.Equal(587, options.Smtp.Port);
            Assert.Equal(EmailAuthenticationType.Plain, options.Authentication.Type);
            Assert.Equal("override-user", options.Authentication.Plain.Username);
            Assert.Equal("sender@override.test", options.Sender.Address);
            Assert.Equal(100, options.DeliveryPolicy.MaxRecipients);
        }
    }
}
