using Common.Transfer.Http;

namespace Common.Test.Transfer.Http;

public sealed class HttpCallTest
{
    [Fact]
    public async Task HttpCall_Should_Propagate_Transport_Failure()
    {
        var expected = new HttpRequestException("Transport failed.");
        using var client = new HttpClient(new StubHttpMessageHandler(
            (_, _) => Task.FromException<HttpResponseMessage>(expected)));

        var actual = await Assert.ThrowsAsync<HttpRequestException>(() => new HttpCall(client).Get(
            "https://example.test/",
            cancellationToken: TestContext.Current.CancellationToken));

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task HttpCall_Should_Combine_Base_Url_Path_And_Query()
    {
        Uri? requestUri = null;
        using var client = new HttpClient(new StubHttpMessageHandler((request, _) =>
        {
            requestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage());
        }));

        using var response = await new HttpCall(client).Get(
            "https://example.test/api/",
            "status",
            new Dictionary<string, object> { ["page"] = 2 },
            TestContext.Current.CancellationToken);

        Assert.Equal(new Uri("https://example.test/api/status?page=2"), requestUri);
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return sendAsync(request, cancellationToken);
        }
    }
}
