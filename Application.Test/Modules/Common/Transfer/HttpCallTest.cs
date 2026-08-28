using Common.Transfer.Http;

namespace Application.Test.Modules.Common.Transfer
{
    public class HttpCallTest
    {
        [Fact]
        public void BuildUri_Should_Append_Query_Parameters()
        {
            var httpCall = new HttpCall();

            string uri = httpCall.BuildUri("status", new()
            {
                ["name"] = "Command Q",
                ["page"] = 2
            });

            Assert.Equal("status?name=Command%20Q&page=2", uri);
        }

        [Fact]
        public void AddHeaders_Should_Store_Request_Headers()
        {
            var httpCall = new HttpCall()
                .AddHeader("X-Test", "value")
                .AddContentHeader("X-Content", "content");

            Assert.Equal("value", httpCall.Headers["X-Test"]);
            Assert.Equal("content", httpCall.ContentHeaders["X-Content"]);
        }

        [Fact]
        public async Task CreateJsonContent_Should_Use_Json_Media_Type()
        {
            var httpCall = new HttpCall();

            using var content = httpCall.CreateJsonContent(new { name = "command-q" });

            Assert.Equal("application/json", content.Headers.ContentType?.MediaType);
            Assert.Contains("command-q", await content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        }
    }
}
