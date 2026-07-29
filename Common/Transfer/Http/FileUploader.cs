using System.Net;
using System.Net.Mime;
using Common.Models.Transfer;

namespace Common.Transfer.Http
{
    public sealed class FileUploader
    {
        public Dictionary<string, string> Headers { get; } = [];

        public Dictionary<string, string> ContentHeaders { get; } = [];

        public FileUploader()
        {
        }

        public FileUploader(Dictionary<string, string> headers)
        {
            AddHeaders(headers);
        }

        public FileUploader AddHeader(string name, string value)
        {
            Headers[name] = value;

            return this;
        }

        public FileUploader AddHeaders(Dictionary<string, string> headers)
        {
            foreach (var header in headers)
                AddHeader(header.Key, header.Value);

            return this;
        }

        public FileUploader AddContentHeader(string name, string value)
        {
            ContentHeaders[name] = value;

            return this;
        }

        public FileUploader AddContentHeaders(Dictionary<string, string> headers)
        {
            foreach (var header in headers)
                AddContentHeader(header.Key, header.Value);

            return this;
        }

        public FileUploader ClearHeaders()
        {
            Headers.Clear();

            return this;
        }

        public FileUploader ClearContentHeaders()
        {
            ContentHeaders.Clear();

            return this;
        }

        public async Task<HttpResponseMessage> UploadFile(string baseUrl, string filePath, string? path = null, string name = "file", Dictionary<string, string>? fields = null, string contentType = MediaTypeNames.Application.Octet, Dictionary<string, object>? queryParams = null, CancellationToken cancellationToken = default)
        {
            await using var file = File.OpenRead(filePath);

            var item = new FileUploadItem
            {
                Content = file,
                FileName = Path.GetFileName(filePath),
                Name = name,
                ContentType = contentType
            };

            return await Upload(baseUrl, item, path, fields, queryParams, cancellationToken);
        }

        public async Task<HttpResponseMessage> UploadStream(string baseUrl, Stream stream, string fileName, string? path = null, string name = "file", Dictionary<string, string>? fields = null, string contentType = MediaTypeNames.Application.Octet, Dictionary<string, object>? queryParams = null, CancellationToken cancellationToken = default)
        {
            var item = new FileUploadItem
            {
                Content = stream,
                FileName = fileName,
                Name = name,
                ContentType = contentType
            };

            return await Upload(baseUrl, item, path, fields, queryParams, cancellationToken);
        }

        public async Task<HttpResponseMessage> UploadBytes(string baseUrl, byte[] bytes, string fileName, string? path = null, string name = "file", Dictionary<string, string>? fields = null, string contentType = MediaTypeNames.Application.Octet, Dictionary<string, object>? queryParams = null, CancellationToken cancellationToken = default)
        {
            await using var stream = new MemoryStream(bytes);

            return await UploadStream(baseUrl, stream, fileName, path, name, fields, contentType, queryParams, cancellationToken);
        }

        public async Task<HttpResponseMessage> Upload(string baseUrl, FileUploadItem file, string? path = null, Dictionary<string, string>? fields = null, Dictionary<string, object>? queryParams = null, CancellationToken cancellationToken = default)
        {
            return await Upload(baseUrl, [file], path, fields, queryParams, cancellationToken);
        }

        public async Task<HttpResponseMessage> Upload(string baseUrl, IEnumerable<FileUploadItem> files, string? path = null, Dictionary<string, string>? fields = null, Dictionary<string, object>? queryParams = null, CancellationToken cancellationToken = default)
        {
            using var content = CreateMultipartContent(files, fields);
            using HttpClient client = new()
            {
                BaseAddress = new(baseUrl),
            };

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, new HttpCall().BuildUri(path, queryParams))
                {
                    Content = content
                };

                ApplyHeaders(request);

                return await client.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);

                return new HttpResponseMessage(HttpStatusCode.Conflict);
            }
        }

        public MultipartFormDataContent CreateMultipartContent(IEnumerable<FileUploadItem> files, Dictionary<string, string>? fields = null)
        {
            var multipart = new MultipartFormDataContent();

            foreach (var field in fields ?? [])
                multipart.Add(new StringContent(field.Value), field.Key);

            foreach (var file in files)
            {
                var streamContent = new StreamContent(file.Content);
                streamContent.Headers.ContentType = new(file.ContentType);
                ApplyContentHeaders(streamContent);

                foreach (var header in file.Headers)
                    streamContent.Headers.TryAddWithoutValidation(header.Key, header.Value);

                multipart.Add(streamContent, file.Name, file.FileName);
            }

            return multipart;
        }

        private void ApplyHeaders(HttpRequestMessage request)
        {
            foreach (var header in Headers)
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        private void ApplyContentHeaders(HttpContent content)
        {
            foreach (var header in ContentHeaders)
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }
}
