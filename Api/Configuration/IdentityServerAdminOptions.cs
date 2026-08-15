namespace Api.Configuration
{
    internal sealed class IdentityServerAdminOptions
    {
        public bool Enabled { get; set; }
        public Uri BaseUrl { get; set; } = new("https://localhost:7125/");
        public string ApiKeyHeaderName { get; set; } = "X-CommandQ-Identity-Admin-Key";
        public string ApiKey { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 15;
        public string UsersPath { get; set; } = "internal/account-admin/users";
        public string RolesPath { get; set; } = "internal/account-admin/roles";
    }
}
