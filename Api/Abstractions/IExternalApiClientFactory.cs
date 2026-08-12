namespace Api.Abstractions
{
    /// <summary>
    /// Creates configured HTTP clients for named external services.
    /// </summary>
    public interface IExternalApiClientFactory
    {
        /// <summary>
        /// Creates a client using the current configuration for the named service.
        /// </summary>
        /// <param name="name">The configured external client name.</param>
        /// <returns>A configured HTTP client.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the client is missing or invalid.</exception>
        HttpClient Create(string name);
    }
}
