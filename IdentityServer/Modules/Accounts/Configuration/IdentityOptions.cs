namespace IdentityServer.Modules.Accounts.Configuration
{
    internal sealed class IdentityConfigurationOptions
    {
        public int RequiredPasswordLength { get; init; }
        public bool RequireDigit { get; init; }
        public bool RequireLowercase { get; init; }
        public bool RequireNonAlphanumeric { get; init; }
        public bool RequireUppercase { get; init; }
        public int MaxFailedAccessAttempts { get; init; }
        public int LockoutMinutes { get; init; }
        public bool RequireConfirmedEmail { get; init; }
        public int CookieHours { get; init; }
        public string CookieName { get; init; } = string.Empty;
        public int AuthenticationPermitLimit { get; init; }
        public int AuthenticationWindowSeconds { get; init; }
        public int AuthenticationQueueLimit { get; init; }
    }

    internal sealed class ExternalProviderOptions
    {
        public ExternalProviderCredentials Google { get; init; } = new();
        public ExternalProviderCredentials Microsoft { get; init; } = new();
    }

    internal sealed class ExternalProviderCredentials
    {
        public string ClientId { get; init; } = string.Empty;
        public string ClientSecret { get; init; } = string.Empty;
    }
}
