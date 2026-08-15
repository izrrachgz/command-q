namespace Api.Configuration
{
    /// <summary>
    /// Defines browser origins allowed to call the API through CORS.
    /// </summary>
    internal sealed record CorsOptions
    {
        /// <summary>
        /// Enables CORS outside Development when a deployment explicitly needs cross-origin browser access.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Exact scheme, host and port origins that may call the API from a browser.
        /// </summary>
        public string[] AllowedOrigins { get; set; } = [];
    }
}
