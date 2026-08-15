using Api.Configuration;
using Api.Test.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Api.Test.Modules.Common.Middleware
{
    public sealed class CorsIntegrationTest(CommandQApiFactory factory) : IClassFixture<CommandQApiFactory>
    {
        [Fact]
        public async Task Preflight_Should_Return_Cors_Headers_When_Cors_Is_Configured()
        {
            var options = factory.Services.GetRequiredService<IOptions<CorsOptions>>().Value;
            Assert.True(options.Enabled);
            Assert.Contains(CommandQApiFactory.ApiClientOrigin, options.AllowedOrigins);

            using var client = factory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Options, CommandQApiFactory.AccountCreatePath);
            request.Headers.Add(HeaderNames.Origin, CommandQApiFactory.ApiClientOrigin);
            request.Headers.Add(HeaderNames.AccessControlRequestMethod, HttpMethod.Post.Method);
            request.Headers.Add(
                HeaderNames.AccessControlRequestHeaders,
                $"{HeaderNames.Authorization}, {CommandQApiFactory.ApiClientHeaderName}");

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.True(
                response.Headers.Contains(HeaderNames.AccessControlAllowOrigin),
                $"{response.StatusCode}; {string.Join("; ", response.Headers.Select(header => $"{header.Key}={string.Join(",", header.Value)}"))}");
            Assert.Equal(CommandQApiFactory.ApiClientOrigin, response.Headers.GetValues(HeaderNames.AccessControlAllowOrigin).Single());
            Assert.Contains(HttpMethod.Post.Method, response.Headers.GetValues(HeaderNames.AccessControlAllowMethods).Single());
        }
    }
}
