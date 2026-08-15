namespace IdentityServer.Configuration
{
    /// <summary>
    /// Defines browser origins allowed to call IdentityServer endpoints through CORS.
    /// </summary>
    internal sealed record CorsOptions
    {
        /// <summary>
        /// Enables CORS outside Development when an external browser application requires it.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Exact scheme, host and port origins that may call IdentityServer from a browser.
        /// </summary>
        public string[] AllowedOrigins { get; set; } = [];
    }
}
