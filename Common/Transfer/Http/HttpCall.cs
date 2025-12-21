#nullable enable
using System.Web;
using System.Net;
using System.Text.Json;
using System.Net.Http.Json;
using System.Net.Mime;

namespace Common.Transfer.Http
{
    public sealed class HttpCall
    {
        public JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public Dictionary<string, string>? ContentHeaders { get; private set; }

        public HttpCall()
        {

        }

        public HttpCall(Dictionary<string, string> contentHeaders) : base()
        {
            ContentHeaders = contentHeaders;
        }

        public async Task<HttpResponseMessage> Get(string baseUrl, string? method = null, Dictionary<string, object>? queryParams = null, CancellationToken cancellationToken = default)
        {
            HttpClient client = new()
            {
                BaseAddress = new(baseUrl),
            };

            try
            {
                string query = string.Empty;

                if (queryParams is not null)
                {
                    query = HttpUtility.UrlEncode(JsonSerializer.SerializeToUtf8Bytes(queryParams));
                }

                if (ContentHeaders is not null && ContentHeaders.Count != 0)
                {
                    client.DefaultRequestHeaders.Clear();
                    foreach (var item in ContentHeaders)
                    {
                        client.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }
                }

                HttpResponseMessage response = await client.GetAsync(method + query, cancellationToken);

                return response;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);

                return new HttpResponseMessage(HttpStatusCode.Conflict);
            }
        }

        public async Task<T?> GetAndReadFromJson<T>(string baseUrl, string? method = null, Dictionary<string, object>? queryParams = null, CancellationToken cancellationToken = default)
        {
            var response = await Get(baseUrl, method, queryParams, cancellationToken);

            try
            {
                return await response.Content.ReadFromJsonAsync<T?>(SerializerOptions, cancellationToken);
            }
            catch
            {
                return default;
            }
        }

        public async Task<HttpResponseMessage> Post(string baseUrl, string? method = null, object? bodyParams = null, CancellationToken cancellationToken = default)
        {
            HttpClient client = new()
            {
                BaseAddress = new(baseUrl),
            };

            try
            {
                HttpContent content = new StringContent(string.Empty);
                if (bodyParams is not null)
                {
                    string json = JsonSerializer.Serialize(bodyParams, options: SerializerOptions);
                    content = new StringContent(json, mediaType: new(MediaTypeNames.Application.Json));
                }

                if (ContentHeaders is not null && ContentHeaders.Count != 0)
                {
                    client.DefaultRequestHeaders.Clear();
                    foreach (var item in ContentHeaders)
                    {
                        client.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }
                }

                HttpResponseMessage response = await client.PostAsync(method, content, cancellationToken);

                return response;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);

                return new HttpResponseMessage(HttpStatusCode.Conflict);
            }
        }

        public async Task<T?> PostAndReadFromJson<T>(string baseUrl, string? method = null, object? bodyParams = null, CancellationToken cancellationToken = default)
        {
            var response = await Post(baseUrl, method, bodyParams, cancellationToken);

            try
            {
                return await response.Content.ReadFromJsonAsync<T?>(SerializerOptions, cancellationToken);
            }
            catch
            {
                return default;
            }
        }
    }
}
