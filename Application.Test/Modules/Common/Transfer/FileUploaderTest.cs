using System.Net.Mime;
using Common.Transfer.Http;
using Common.Models.Transfer;

namespace Application.Test.Modules.Common.Transfer
{
    public class FileUploaderTest
    {
        [Fact]
        public void AddHeaders_Should_Store_Request_And_Content_Headers()
        {
            var uploader = new FileUploader()
                .AddHeader("X-Test", "value")
                .AddContentHeader("X-Content", "content");

            Assert.Equal("value", uploader.Headers["X-Test"]);
            Assert.Equal("content", uploader.ContentHeaders["X-Content"]);
        }

        [Fact]
        public async Task CreateMultipartContent_Should_Add_Fields_And_File()
        {
            var uploader = new FileUploader();
            await using var stream = new MemoryStream("CommandQ"u8.ToArray());

            using var content = uploader.CreateMultipartContent([
                new FileUploadItem
                {
                    Content = stream,
                    FileName = "commandq.txt",
                    Name = "document",
                    ContentType = MediaTypeNames.Text.Plain
                }
            ], new()
            {
                ["source"] = "test"
            });

            string body = await content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            Assert.Equal("multipart/form-data", content.Headers.ContentType?.MediaType);
            Assert.Contains("name=source", body);
            Assert.Contains("test", body);
            Assert.Contains("name=document", body);
            Assert.Contains("filename=commandq.txt", body);
            Assert.Contains("CommandQ", body);
        }

        [Fact]
        public void CreateMultipartContent_Should_Apply_Content_Headers()
        {
            var uploader = new FileUploader()
                .AddContentHeader("X-Content", "content");

            using var stream = new MemoryStream("CommandQ"u8.ToArray());
            using var content = uploader.CreateMultipartContent([
                new FileUploadItem
                {
                    Content = stream,
                    FileName = "commandq.txt",
                    ContentType = MediaTypeNames.Text.Plain,
                    Headers = new()
                    {
                        ["X-File"] = "file"
                    }
                }
            ]);

            var fileContent = Assert.Single(content).Headers;

            Assert.Equal("text/plain", fileContent.ContentType?.MediaType);
            Assert.True(fileContent.TryGetValues("X-Content", out var contentHeaderValues));
            Assert.Equal("content", Assert.Single(contentHeaderValues));
            Assert.True(fileContent.TryGetValues("X-File", out var fileHeaderValues));
            Assert.Equal("file", Assert.Single(fileHeaderValues));
        }
    }
}
