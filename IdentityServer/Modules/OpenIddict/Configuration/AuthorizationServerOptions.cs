namespace IdentityServer.Modules.OpenIddict.Configuration
{
    internal sealed class AuthorizationServerOptions
    {
        public string Issuer { get; init; } = string.Empty;
        public string ApiScope { get; init; } = string.Empty;
        public int AccessTokenMinutes { get; init; }
        public int RefreshTokenDays { get; init; }
        public CertificateOptions Certificates { get; init; } = new();
        public IReadOnlyList<OpenIddictClientOptions> Clients { get; init; } = [];
    }

    internal sealed class CertificateOptions
    {
        public bool UseDevelopmentCertificates { get; init; }
        public CertificateFileOptions Signing { get; init; } = new();
        public CertificateFileOptions Encryption { get; init; } = new();
    }

    internal sealed class CertificateFileOptions
    {
        public string Path { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }

    internal sealed class OpenIddictClientOptions
    {
        public string ClientId { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public string ClientSecret { get; init; } = string.Empty;
        public bool AllowClientCredentials { get; init; }
        public IReadOnlyList<string> RedirectUris { get; init; } = [];
        public IReadOnlyList<string> PostLogoutRedirectUris { get; init; } = [];
    }
}
