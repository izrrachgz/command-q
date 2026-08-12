using Api.Abstractions;
using Api.Configuration;
using Microsoft.Extensions.Options;

namespace Api.Services
{
    /// <summary>
    /// Creates external HTTP clients using the latest endpoint and timeout settings.
    /// </summary>
    internal sealed class ExternalApiClientFactory(IHttpClientFactory clientFactory, IOptionsMonitor<ExternalApiClientsOptions> options) : IExternalApiClientFactory
    {
        /// <inheritdoc />
        public HttpClient Create(string name)
        {
            if (!options.CurrentValue.Clients.TryGetValue(name, out var clientOptions))
                throw new InvalidOperationException($"External API client '{name}' is not configured.");

            if (!Uri.TryCreate(clientOptions.BaseUrl, UriKind.Absolute, out var baseUri))
                throw new InvalidOperationException($"External API client '{name}' has an invalid base URL.");

            var client = clientFactory.CreateClient(name);
            client.BaseAddress = baseUri;
            client.Timeout = TimeSpan.FromSeconds(clientOptions.TimeoutSeconds);

            return client;
        }
    }
}
