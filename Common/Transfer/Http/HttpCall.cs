using System.Net;
using System.Text;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Mime;

namespace Common.Transfer.Http
{
    public sealed class HttpCall
    {
        public JsonSerializerOptions SerializerOptions { get; set; } = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public Dictionary<string, string> Headers { get; } = [];

        public Dictionary<string, string> ContentHeaders { get; } = [];

        public HttpCall()
        {
        }

        public HttpCall(Dictionary<string, string> headers)
        {
            AddHeaders(headers);
        }

        public HttpCall AddHeader(string name, string value)
        {
            Headers[name] = value;

            return this;
        }

        public HttpCall AddHeaders(Dictionary<string, string> headers)
        {
            foreach (var header in headers)
                AddHeader(header.Key, header.Value);

            return this;
        }

        public HttpCall AddContentHeader(string name, string value)
        {
            ContentHeaders[name] = value;

            return this;
        }

        public HttpCall AddContentHeaders(Dictionary<string, string> headers)
        {
            foreach (var header in headers)
                AddContentHeader(header.Key, header.Value);

            return this;
        }

        public HttpCall ClearHeaders()
        {
            Headers.Clear();

            return this;
        }

        public HttpCall ClearContentHeaders()
        {
            ContentHeaders.Clear();

            return this;
        }

        public async Task<HttpResponseMessage> Get(string baseUrl, string? method = null, Dictionary<string, object>? queryParams = null, CancellationToken cancellationToken = default)
        {
            return await Send(HttpMethod.Get, baseUrl, method, queryParams: queryParams, cancellationToken: cancellationToken);
        }

        public async Task<T?> GetAndReadFromJson<T>(string baseUrl, string? method = null, Dictionary<string, object>? queryParams = null, CancellationToken cancellationToken = default)
        {
            var response = await Get(baseUrl, method, queryParams, cancellationToken);

            return await ReadFromJson<T>(response, cancellationToken);
        }

        public async Task<HttpResponseMessage> Post(string baseUrl, string? method = null, object? bodyParams = null, CancellationToken cancellationToken = default)
        {
            return await Send(HttpMethod.Post, baseUrl, method, content: CreateJsonContent(bodyParams), cancellationToken: cancellationToken);
        }

        public async Task<T?> PostAndReadFromJson<T>(string baseUrl, string? method = null, object? bodyParams = null, CancellationToken cancellationToken = default)
        {
            var response = await Post(baseUrl, method, bodyParams, cancellationToken);

            return await ReadFromJson<T>(response, cancellationToken);
        }

        public async Task<HttpResponseMessage> PostForm(string baseUrl, string? method = null, Dictionary<string, string>? form = null, CancellationToken cancellationToken = default)
        {
            return await Send(HttpMethod.Post, baseUrl, method, content: CreateFormContent(form), cancellationToken: cancellationToken);
        }

        public async Task<HttpResponseMessage> PostString(string baseUrl, string? method = null, string content = "", string mediaType = MediaTypeNames.Text.Plain, CancellationToken cancellationToken = default)
        {
            return await Send(HttpMethod.Post, baseUrl, method, content: CreateStringContent(content, mediaType), cancellationToken: cancellationToken);
        }

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
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);

                return new HttpResponseMessage(HttpStatusCode.Conflict);
            }
        }

        public HttpContent CreateJsonContent(object? body)
        {
            if (body is null)
                return new StringContent(string.Empty);

            string json = JsonSerializer.Serialize(body, SerializerOptions);

            return CreateStringContent(json, MediaTypeNames.Application.Json);
        }

        public HttpContent CreateFormContent(Dictionary<string, string>? form)
        {
            return new FormUrlEncodedContent(form ?? []);
        }

        public HttpContent CreateStringContent(string content, string mediaType = MediaTypeNames.Text.Plain)
        {
            return new StringContent(content, Encoding.UTF8, mediaType);
        }

        public string BuildUri(string? path = null, Dictionary<string, object>? queryParams = null)
        {
            string uri = path ?? string.Empty;
            string query = BuildQueryString(queryParams);

            if (string.IsNullOrWhiteSpace(query))
                return uri;

            return uri.Contains('?') ? $@"{uri}&{query}" : $@"{uri}?{query}";
        }

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
