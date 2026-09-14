namespace IdentityServer.Modules.Accounts.Configuration
{
    /// <summary>Configures the API-key-protected endpoints used by the command-q API for identity administration.</summary>
    public sealed class IdentityAdminApiOptions
    {
        /// <summary>Gets or sets whether the internal identity-administration endpoints accept requests.</summary>
        public bool Enabled { get; set; }
        /// <summary>Gets or sets the HTTP header that carries the internal API key.</summary>
        public string ApiKeyHeaderName { get; set; } = "X-command-q-Identity-Admin-Key";
        /// <summary>Gets or sets the internal API key expected from the command-q API.</summary>
        public string ApiKey { get; set; } = string.Empty;
    }
}
