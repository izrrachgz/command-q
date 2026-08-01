using System.Net;
using Api.Test.Infrastructure;
using Microsoft.Net.Http.Headers;

namespace Api.Test.Modules.Identity
{
    public sealed class AccountControllerIntegrationTest(CommandQApiFactory factory) : IClassFixture<CommandQApiFactory>
    {
        [Fact]
        public async Task Me_Should_Return_Unauthorized_When_Request_Has_Valid_Api_Client_But_No_Access_Token()
        {
            using var client = factory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, "/Account/me");
            request.Headers.Add(CommandQApiFactory.ApiClientHeaderName, CommandQApiFactory.ApiClientHeaderValue);
            request.Headers.Add(HeaderNames.Origin, CommandQApiFactory.ApiClientOrigin);

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Me_Should_Return_Forbidden_When_Api_Client_Header_Is_Missing()
        {
            using var client = factory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, "/Account/me");
            request.Headers.Add(HeaderNames.Origin, CommandQApiFactory.ApiClientOrigin);

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
