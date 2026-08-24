using Api.Constants;
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
            using var request = new HttpRequestMessage(HttpMethod.Get, CommandQApiFactory.AccountMePath);
            request.Headers.Add(CommandQApiFactory.ApiClientHeaderName, CommandQApiFactory.ApiClientHeaderValue);
            request.Headers.Add(HeaderNames.Origin, CommandQApiFactory.ApiClientOrigin);

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(SecurityHeaderValues.NoSniff, response.Headers.GetValues(HeaderNames.XContentTypeOptions).Single());
            Assert.Equal(SecurityHeaderValues.Deny, response.Headers.GetValues(HeaderNames.XFrameOptions).Single());
            Assert.Equal(SecurityHeaderValues.NoReferrer, response.Headers.GetValues(SecurityHeaderNames.ReferrerPolicy).Single());
            Assert.Equal(
                SecurityHeaderValues.None,
                response.Headers.GetValues(SecurityHeaderNames.XPermittedCrossDomainPolicies).Single());
        }
    }
}
