using Api.Configuration;
using Api.Test.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Api.Test.Modules.Common.Middleware
{
    public sealed class CorsIntegrationTest(ApiFactory factory) : IClassFixture<ApiFactory>
    {
        [Fact]
        public async Task Preflight_Should_Return_Cors_Headers_When_Cors_Is_Configured()
        {
            var options = factory.Services.GetRequiredService<IOptions<CorsOptions>>().Value;
            Assert.True(options.Enabled);
            Assert.Contains(ApiFactory.ApiClientOrigin, options.AllowedOrigins);

            using var client = factory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Options, ApiFactory.AccountCreatePath);
            request.Headers.Add(HeaderNames.Origin, ApiFactory.ApiClientOrigin);
            request.Headers.Add(HeaderNames.AccessControlRequestMethod, HttpMethod.Post.Method);
            request.Headers.Add(
                HeaderNames.AccessControlRequestHeaders,
                $"{HeaderNames.Authorization}, {ApiFactory.ApiClientHeaderName}");

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.True(
                response.Headers.Contains(HeaderNames.AccessControlAllowOrigin),
                $"{response.StatusCode}; {string.Join("; ", response.Headers.Select(header => $"{header.Key}={string.Join(",", header.Value)}"))}");
            Assert.Equal(ApiFactory.ApiClientOrigin, response.Headers.GetValues(HeaderNames.AccessControlAllowOrigin).Single());
            Assert.Contains(HttpMethod.Post.Method, response.Headers.GetValues(HeaderNames.AccessControlAllowMethods).Single());
        }
    }
}
