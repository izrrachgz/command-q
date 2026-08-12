namespace Api.Configuration
{
    /// <summary>
    /// Configuration for all named HTTP clients used to call external services.
    /// </summary>
    public sealed class ExternalApiClientsOptions
    {
        /// <summary>
        /// Gets the named external client configurations.
        /// </summary>
        public Dictionary<string, ExternalApiClientOptions> Clients { get; init; } = [];
    }

    /// <summary>
    /// Connection settings for one external HTTP service.
    /// </summary>
    public sealed class ExternalApiClientOptions
    {
        /// <summary>
        /// Gets the absolute base URL used by the client.
        /// </summary>
        public string BaseUrl { get; init; } = string.Empty;

        /// <summary>
        /// Gets the maximum duration allowed for an HTTP request.
        /// </summary>
        public int TimeoutSeconds { get; init; } = 30;
    }
}
