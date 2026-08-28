using Api.Test.Infrastructure;
using Application.Modules.Health;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.Test.Modules.Health
{
    public sealed class HealthEndpointIntegrationTest(ApiFactory factory) : IClassFixture<ApiFactory>
    {
        [Fact]
        public async Task Live_Health_Endpoint_Should_Not_Require_Api_Client()
        {
            using var client = factory.CreateClient();

            using var response = await client.GetAsync("/health/live", TestContext.Current.CancellationToken);

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public void Health_Check_Registrations_Should_Use_Purpose_Tags()
        {
            var options = factory.Services.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;
            var registrations = options.Registrations.ToDictionary(registration => registration.Name);

            Assert.Contains(HealthCheckConstants.LiveTag, registrations[HealthCheckConstants.SelfName].Tags);
            Assert.Contains(HealthCheckConstants.ReadyTag, registrations[HealthCheckConstants.DatabaseReadName].Tags);
            Assert.DoesNotContain(HealthCheckConstants.ReadyTag, registrations[HealthCheckConstants.DatabaseWriteName].Tags);
            Assert.Contains(HealthCheckConstants.StartupTag, registrations[HealthCheckConstants.DatabaseWriteName].Tags);
            Assert.Contains(HealthCheckConstants.PublishTag, registrations[HealthCheckConstants.InternetAccessName].Tags);
        }
    }
}
