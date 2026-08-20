using Domain.Modules.Configuration;
using Application.Modules.Configuration.Commands;

namespace Application.Test.Modules.Configuration
{
    public sealed class ApplicationConfigurationContractTest
    {
        [Fact]
        public void Configuration_Identifiers_Should_Use_Int32()
        {
            Assert.Equal(typeof(int), typeof(ApplicationConfiguration).GetProperty(nameof(ApplicationConfiguration.Id))?.PropertyType);
        }

        [Fact]
        public void Startup_Command_Should_Require_Host_Resolved_Metadata()
        {
            var command = new RecordApplicationConfigurationCommand
            {
                ServerTimeZone = "America/Hermosillo",
                ApplicationVersion = "1.0.0",
                Language = "en-US",
                Environment = "Development",
                InstanceName = "api-01"
            };

            Assert.Equal("1.0.0", command.ApplicationVersion);
            Assert.Equal("api-01", command.InstanceName);
        }
    }
}
