using System.Text.Json;
using Common.Transfer.Http;
using System.Text.Json.Serialization.Metadata;

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

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Json_Helpers_Should_Dispose_Response_Content(bool post, bool usesTypeInfo)
    {
        using var content = new TrackingContent("42");
        using var client = new HttpClient(new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage { Content = content })));
        var call = new HttpCall(client);
        var cancellationToken = TestContext.Current.CancellationToken;
        var metadata = (JsonTypeInfo<int>)JsonSerializerOptions.Web.GetTypeInfo(typeof(int));

        int value = (post, usesTypeInfo) switch
        {
            (false, false) => await call.GetAndReadFromJson<int>("https://example.test/", cancellationToken: cancellationToken),
            (false, true) => await call.GetAndReadFromJson("https://example.test/", metadata, cancellationToken: cancellationToken),
            (true, false) => await call.PostAndReadFromJson<int>("https://example.test/", cancellationToken: cancellationToken),
            (true, true) => await call.PostAndReadFromJson("https://example.test/", metadata, cancellationToken: cancellationToken)
        };

        Assert.Equal(42, value);
        Assert.True(content.WasDisposed);
    }

    private sealed class TrackingContent(string content) : StringContent(content)
    {
        public bool WasDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            WasDisposed = true;
            base.Dispose(disposing);
        }
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
