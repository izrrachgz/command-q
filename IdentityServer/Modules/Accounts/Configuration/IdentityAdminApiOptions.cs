namespace IdentityServer.Modules.Accounts.Configuration
{
    public sealed class IdentityAdminApiOptions
    {
        public bool Enabled { get; set; }
        public string ApiKeyHeaderName { get; set; } = "X-CommandQ-Identity-Admin-Key";
        public string ApiKey { get; set; } = string.Empty;
    }
}
