using System.Net;
using Api.Test.Infrastructure;
using Email.Client.Abstractions;
using Email.Client.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Net.Http.Headers;

namespace Api.Test.Modules.Notifications;

public sealed class NotificationEmailsAuthorizationIntegrationTest(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Path = "/notifications/emails";

    [Fact]
    public async Task Search_Should_Return_Unauthorized_Without_Access_Token()
    {
        using HttpClient client = CreateClient();
        using HttpRequestMessage request = CreateRequest();

        using HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Search_Should_Return_Forbidden_For_Non_Admin()
    {
        using HttpClient client = CreateClient();
        using HttpRequestMessage request = CreateRequest(ApiFactory.CreateAccessToken());

        using HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Search_Should_Return_Ok_For_Admin()
    {
        using HttpClient client = CreateClient();
        using HttpRequestMessage request = CreateRequest(ApiFactory.CreateAccessToken(roles: ["Admin"]));

        using HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private HttpClient CreateClient() => factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
    {
        services.RemoveAll<IEmailQueueClient>();
        services.AddSingleton<IEmailQueueClient, EmptyEmailQueueClient>();
    })).CreateClient();

    private static HttpRequestMessage CreateRequest(string? token = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, Path);
        request.Headers.Add(ApiFactory.ApiClientHeaderName, ApiFactory.ApiClientHeaderValue);
        request.Headers.Add(HeaderNames.Origin, ApiFactory.ApiClientOrigin);
        if (token is not null) request.Headers.Authorization = new("Bearer", token);
        return request;
    }

    private sealed class EmptyEmailQueueClient : IEmailQueueClient
    {
        public Task<EmailQueueResult> EnqueueAsync(EmailQueueRequest request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<EmailQueuePageResult> GetPageAsync(EmailQueuePageRequest request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<EmailQueuePageResult> SearchAsync(EmailQueueSearchRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(new EmailQueuePageResult
            {
                Items = [],
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = 0,
                TotalPages = 0
            });

        public Task<QueuedEmailDetail?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<QueuedEmailDetail?>(null);
    }
}
