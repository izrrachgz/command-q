using Api.Test.Infrastructure;
using Microsoft.Net.Http.Headers;

namespace Api.Test.Modules.Common.Middleware
{
    public sealed class SecurityHeadersIntegrationTest(CommandQApiFactory factory) : IClassFixture<CommandQApiFactory>
    {
        [Fact]
        public async Task Api_Response_Should_Include_Security_Headers()
        {
            using var client = factory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, "/Account/me");
            request.Headers.Add(CommandQApiFactory.ApiClientHeaderName, CommandQApiFactory.ApiClientHeaderValue);
            request.Headers.Add(HeaderNames.Origin, CommandQApiFactory.ApiClientOrigin);

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
            Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
            Assert.Equal("no-referrer", response.Headers.GetValues("Referrer-Policy").Single());
            Assert.Equal("none", response.Headers.GetValues("X-Permitted-Cross-Domain-Policies").Single());
        }
    }
}
