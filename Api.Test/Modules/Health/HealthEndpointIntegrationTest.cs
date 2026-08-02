using Api.Test.Infrastructure;

namespace Api.Test.Modules.Health
{
    public sealed class HealthEndpointIntegrationTest(CommandQApiFactory factory) : IClassFixture<CommandQApiFactory>
    {
        [Fact]
        public async Task Live_Health_Endpoint_Should_Not_Require_Api_Client()
        {
            using var client = factory.CreateClient();

            using var response = await client.GetAsync("/health/live", TestContext.Current.CancellationToken);

            response.EnsureSuccessStatusCode();
        }
    }
}
