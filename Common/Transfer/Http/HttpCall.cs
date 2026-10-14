using System.Net;
using System.Text;
using System.Net.Mime;
using System.Text.Json;
using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;

namespace Common.Transfer.Http
{
    /// <summary>
    /// Small helper for sending HTTP requests with reusable headers, content headers and JSON serialization settings.
    /// </summary>
    public sealed class HttpCall
    {
        /// <summary>
        /// Serializer settings used when writing JSON request bodies and reading JSON responses.
        /// </summary>
        public JsonSerializerOptions SerializerOptions { get; set; } = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// Headers applied to each outgoing request.
        /// </summary>
        public Dictionary<string, string> Headers { get; } = [];

        /// <summary>
        /// Headers applied to outgoing request content when content is present.
        /// </summary>
        public Dictionary<string, string> ContentHeaders { get; } = [];

        /// <summary>
        /// Creates an HTTP helper without preconfigured headers.
        /// </summary>
        public HttpCall()
        {
        }

        /// <summary>
        /// Creates an HTTP helper with request headers that are applied to every request.
        /// </summary>
        public HttpCall(Dictionary<string, string> headers)
        {
            AddHeaders(headers);
        }

        /// <summary>
        /// Adds or replaces a request header.
        /// </summary>
        public HttpCall AddHeader(string name, string value)
        {
            Headers[name] = value;

            return this;
        }

        /// <summary>
        /// Adds or replaces a set of request headers.
        /// </summary>
        public HttpCall AddHeaders(Dictionary<string, string> headers)
        {
            foreach (var header in headers)
                AddHeader(header.Key, header.Value);

            return this;
        }

        /// <summary>
        /// Adds or replaces a header that is applied to outgoing request content.
        /// </summary>
        public HttpCall AddContentHeader(string name, string value)
        {
            ContentHeaders[name] = value;

            return this;
        }

        /// <summary>
        /// Adds or replaces a set of headers that are applied to outgoing request content.
        /// </summary>
        public HttpCall AddContentHeaders(Dictionary<string, string> headers)
        {
            foreach (var header in headers)
                AddContentHeader(header.Key, header.Value);

            return this;
        }

        /// <summary>
        /// Removes all configured request headers.
        /// </summary>
        public HttpCall ClearHeaders()
        {
            Headers.Clear();

            return this;
        }

        /// <summary>
        /// Removes all configured content headers.
        /// </summary>
        public HttpCall ClearContentHeaders()
        {
            ContentHeaders.Clear();

            return this;
        }

        /// <summary>
        /// Sends a GET request to the base URL and optional relative path.
        /// </summary>
        public async Task<HttpResponseMessage> Get(string baseUrl, string? method = null, Dictionary<string, object>? queryParams = null, CancellationToken cancellationToken = default)
        {
            return await Send(HttpMethod.Get, baseUrl, method, queryParams: queryParams, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Sends a GET request and deserializes the response body as JSON.
        /// </summary>
        public async Task<T?> GetAndReadFromJson<T>(string baseUrl, string? method = null, Dictionary<string, object>? queryParams = null, CancellationToken cancellationToken = default)
        {
            var response = await Get(baseUrl, method, queryParams, cancellationToken);

            return await ReadFromJson<T>(response, cancellationToken);
        }

        /// <summary>
        /// Sends a GET request and deserializes the response body as JSON using source-generated type metadata.
        /// </summary>
        public async Task<T?> GetAndReadFromJson<T>(string baseUrl, JsonTypeInfo<T> jsonTypeInfo, string? method = null, Dictionary<string, object>? queryParams = null, CancellationToken cancellationToken = default)
        {
            var response = await Get(baseUrl, method, queryParams, cancellationToken);

            return await ReadFromJson(response, jsonTypeInfo, cancellationToken);
        }

        /// <summary>
        /// Sends a POST request with a JSON body.
        /// </summary>
        public async Task<HttpResponseMessage> Post(string baseUrl, string? method = null, object? bodyParams = null, CancellationToken cancellationToken = default)
        {
            return await Send(HttpMethod.Post, baseUrl, method, content: CreateJsonContent(bodyParams), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Sends a POST request with a JSON body and deserializes the response body as JSON.
        /// </summary>
        public async Task<T?> PostAndReadFromJson<T>(string baseUrl, string? method = null, object? bodyParams = null, CancellationToken cancellationToken = default)
        {
            var response = await Post(baseUrl, method, bodyParams, cancellationToken);

            return await ReadFromJson<T>(response, cancellationToken);
        }

        /// <summary>
        /// Sends a POST request with a JSON body and deserializes the response body as JSON using source-generated type metadata.
        /// </summary>
        public async Task<T?> PostAndReadFromJson<T>(string baseUrl, JsonTypeInfo<T> jsonTypeInfo, string? method = null, object? bodyParams = null, CancellationToken cancellationToken = default)
        {
            var response = await Post(baseUrl, method, bodyParams, cancellationToken);

            return await ReadFromJson(response, jsonTypeInfo, cancellationToken);
        }

        /// <summary>
        /// Sends a POST request with form-url-encoded content.
        /// </summary>
        public async Task<HttpResponseMessage> PostForm(string baseUrl, string? method = null, Dictionary<string, string>? form = null, CancellationToken cancellationToken = default)
        {
            return await Send(HttpMethod.Post, baseUrl, method, content: CreateFormContent(form), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Sends a POST request with string content and the specified media type.
        /// </summary>
        public async Task<HttpResponseMessage> PostString(string baseUrl, string? method = null, string content = "", string mediaType = MediaTypeNames.Text.Plain, CancellationToken cancellationToken = default)
        {
            return await Send(HttpMethod.Post, baseUrl, method, content: CreateStringContent(content, mediaType), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Sends an HTTP request with optional content and query parameters.
        /// </summary>
        public async Task<HttpResponseMessage> Send(HttpMethod method, string baseUrl, string? path = null, HttpContent? content = null, Dictionary<string, object>? queryParams = null, CancellationToken cancellationToken = default)
        {
            using HttpClient client = new()
            {
                BaseAddress = new(baseUrl),
            };

            try
            {
                using var request = new HttpRequestMessage(method, BuildUri(path, queryParams));

                ApplyHeaders(request);

                if (content is not null)
                {
                    ApplyContentHeaders(content);
                    request.Content = content;
                }

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
        /// Creates JSON HTTP content using <see cref="SerializerOptions"/>.
        /// </summary>
        public HttpContent CreateJsonContent(object? body)
        {
            if (body is null)
                return new StringContent(string.Empty);

            string json = JsonSerializer.Serialize(body, SerializerOptions);

            return CreateStringContent(json, MediaTypeNames.Application.Json);
        }

        /// <summary>
        /// Creates JSON HTTP content using source-generated type metadata.
        /// </summary>
        public HttpContent CreateJsonContent<T>(T body, JsonTypeInfo<T> jsonTypeInfo)
        {
            if (body is null)
                return new StringContent(string.Empty);

            string json = JsonSerializer.Serialize(body, jsonTypeInfo);

            return CreateStringContent(json, MediaTypeNames.Application.Json);
        }

        /// <summary>
        /// Creates form-url-encoded HTTP content.
        /// </summary>
        public HttpContent CreateFormContent(Dictionary<string, string>? form)
        {
            return new FormUrlEncodedContent(form ?? []);
        }

        /// <summary>
        /// Creates string HTTP content with UTF-8 encoding.
        /// </summary>
        public HttpContent CreateStringContent(string content, string mediaType = MediaTypeNames.Text.Plain)
        {
            return new StringContent(content, Encoding.UTF8, mediaType);
        }

        /// <summary>
        /// Builds a relative URI by appending encoded query parameters to the optional path.
        /// </summary>
        public string BuildUri(string? path = null, Dictionary<string, object>? queryParams = null)
        {
            string uri = path ?? string.Empty;
            string query = BuildQueryString(queryParams);

            if (string.IsNullOrWhiteSpace(query))
                return uri;

            return uri.Contains('?') ? $@"{uri}&{query}" : $@"{uri}?{query}";
        }

        /// <summary>
        /// Builds an encoded query string from non-null parameter values.
        /// </summary>
        public string BuildQueryString(Dictionary<string, object>? queryParams)
        {
            if (queryParams is null || queryParams.Count == 0)
                return string.Empty;

            return string.Join("&", queryParams
                .Where(parameter => parameter.Value is not null)
                .Select(parameter => $@"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(Convert.ToString(parameter.Value) ?? string.Empty)}"));
        }

        private async Task<T?> ReadFromJson<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            try
            {
                return await response.Content.ReadFromJsonAsync<T?>(SerializerOptions, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return default;
            }
        }

        private static async Task<T?> ReadFromJson<T>(HttpResponseMessage response, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken)
        {
            try
            {
                return await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return default;
            }
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
