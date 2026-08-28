using System.Net;
using Api.Test.Infrastructure;
using Microsoft.Net.Http.Headers;

namespace Api.Test.Modules.Identity
{
    public sealed class AccountControllerIntegrationTest(ApiFactory factory) : IClassFixture<ApiFactory>
    {
        [Fact]
        public async Task Me_Should_Return_Unauthorized_When_Request_Has_Valid_Api_Client_But_No_Access_Token()
        {
            using var client = factory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, ApiFactory.AccountMePath);
            request.Headers.Add(ApiFactory.ApiClientHeaderName, ApiFactory.ApiClientHeaderValue);
            request.Headers.Add(HeaderNames.Origin, ApiFactory.ApiClientOrigin);

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Me_Should_Return_Forbidden_When_Api_Client_Header_Is_Missing()
        {
            using var client = factory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, ApiFactory.AccountMePath);
            request.Headers.Add(HeaderNames.Origin, ApiFactory.ApiClientOrigin);

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Me_Should_Return_Forbidden_When_Access_Token_Does_Not_Have_Api_Scope()
        {
            using var client = factory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, ApiFactory.AccountMePath);
            request.Headers.Add(ApiFactory.ApiClientHeaderName, ApiFactory.ApiClientHeaderValue);
            request.Headers.Add(HeaderNames.Origin, ApiFactory.ApiClientOrigin);
            request.Headers.Authorization = new("Bearer", ApiFactory.CreateAccessToken(scope: null));

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Me_Should_Return_Unauthorized_When_Access_Token_Has_Wrong_Audience()
        {
            using var client = factory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, ApiFactory.AccountMePath);
            request.Headers.Add(ApiFactory.ApiClientHeaderName, ApiFactory.ApiClientHeaderValue);
            request.Headers.Add(HeaderNames.Origin, ApiFactory.ApiClientOrigin);
            request.Headers.Authorization = new(
                "Bearer",
                ApiFactory.CreateAccessToken(audience: "another_api"));

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
