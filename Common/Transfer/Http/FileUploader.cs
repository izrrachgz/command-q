using System.Net;
using System.Net.Mime;
using Common.Models.Transfer;

namespace Common.Transfer.Http
{
    /// <summary>
    /// Helper for sending multipart/form-data file uploads with reusable request and content headers.
    /// </summary>
    public sealed class FileUploader
    {
        /// <summary>
        /// Headers applied to each upload request.
        /// </summary>
        public Dictionary<string, string> Headers { get; } = [];

        /// <summary>
        /// Headers applied to each file content part.
        /// </summary>
        public Dictionary<string, string> ContentHeaders { get; } = [];

        /// <summary>
        /// Creates a file uploader without preconfigured headers.
        /// </summary>
        public FileUploader()
        {
        }

        /// <summary>
        /// Creates a file uploader with request headers that are applied to every upload request.
        /// </summary>
        public FileUploader(Dictionary<string, string> headers)
        {
            AddHeaders(headers);
        }

        /// <summary>
        /// Adds or replaces a request header.
        /// </summary>
        public FileUploader AddHeader(string name, string value)
        {
            Headers[name] = value;

            return this;
        }

        /// <summary>
        /// Adds or replaces a set of request headers.
        /// </summary>
        public FileUploader AddHeaders(Dictionary<string, string> headers)
        {
            foreach (var header in headers)
                AddHeader(header.Key, header.Value);

            return this;
        }

        /// <summary>
        /// Adds or replaces a header applied to each file content part.
        /// </summary>
        public FileUploader AddContentHeader(string name, string value)
        {
            ContentHeaders[name] = value;

            return this;
        }

        /// <summary>
        /// Adds or replaces a set of headers applied to each file content part.
        /// </summary>
        public FileUploader AddContentHeaders(Dictionary<string, string> headers)
        {
            foreach (var header in headers)
                AddContentHeader(header.Key, header.Value);

            return this;
        }

        /// <summary>
        /// Removes all configured request headers.
        /// </summary>
        public FileUploader ClearHeaders()
        {
            Headers.Clear();

            return this;
        }

        /// <summary>
        /// Removes all configured file content headers.
        /// </summary>
        public FileUploader ClearContentHeaders()
        {
            ContentHeaders.Clear();

            return this;
        }

        /// <summary>
        /// Opens a file from disk and uploads it as a single multipart file part.
        /// </summary>
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

        /// <summary>
        /// Uploads an existing stream as a single multipart file part. The caller owns the stream lifetime.
        /// </summary>
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

        /// <summary>
        /// Uploads a byte array as a single multipart file part.
        /// </summary>
        public async Task<HttpResponseMessage> UploadBytes(string baseUrl, byte[] bytes, string fileName, string? path = null, string name = "file", Dictionary<string, string>? fields = null, string contentType = MediaTypeNames.Application.Octet, Dictionary<string, object>? queryParams = null, CancellationToken cancellationToken = default)
        {
            await using var stream = new MemoryStream(bytes);

            return await UploadStream(baseUrl, stream, fileName, path, name, fields, contentType, queryParams, cancellationToken);
        }

        /// <summary>
        /// Uploads one file item as multipart/form-data.
        /// </summary>
        public async Task<HttpResponseMessage> Upload(string baseUrl, FileUploadItem file, string? path = null, Dictionary<string, string>? fields = null, Dictionary<string, object>? queryParams = null, CancellationToken cancellationToken = default)
        {
            return await Upload(baseUrl, [file], path, fields, queryParams, cancellationToken);
        }

        /// <summary>
        /// Uploads one or more file items as multipart/form-data.
        /// </summary>
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
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);

                return new HttpResponseMessage(HttpStatusCode.Conflict);
            }
        }

        /// <summary>
        /// Creates multipart/form-data content with optional text fields and file parts.
        /// </summary>
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
