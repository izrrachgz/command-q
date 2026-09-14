namespace IdentityServer.Modules.Accounts.Configuration
{
    /// <summary>Configures local credential requirements, lockout behavior, authentication cookies, and sign-in rate limits.</summary>
    internal sealed class IdentityConfigurationOptions
    {
        /// <summary>Gets the minimum accepted password length.</summary>
        public int RequiredPasswordLength { get; init; }
        /// <summary>Gets whether passwords must contain a digit.</summary>
        public bool RequireDigit { get; init; }
        /// <summary>Gets whether passwords must contain a lowercase character.</summary>
        public bool RequireLowercase { get; init; }
        /// <summary>Gets whether passwords must contain a non-alphanumeric character.</summary>
        public bool RequireNonAlphanumeric { get; init; }
        /// <summary>Gets whether passwords must contain an uppercase character.</summary>
        public bool RequireUppercase { get; init; }
        /// <summary>Gets the failed sign-in threshold for ordinary Identity lockout.</summary>
        public int MaxFailedAccessAttempts { get; init; }
        /// <summary>Gets the duration in minutes of an ordinary Identity lockout.</summary>
        public int LockoutMinutes { get; init; }
        /// <summary>Gets whether local sign-in requires a confirmed email address.</summary>
        public bool RequireConfirmedEmail { get; init; }
        /// <summary>Gets the authentication-cookie lifetime in hours.</summary>
        public int CookieHours { get; init; }
        /// <summary>Gets the authentication-cookie name.</summary>
        public string CookieName { get; init; } = string.Empty;
        /// <summary>Gets the number of authentication requests permitted during one rate-limit window.</summary>
        public int AuthenticationPermitLimit { get; init; }
        /// <summary>Gets the authentication rate-limit window in seconds.</summary>
        public int AuthenticationWindowSeconds { get; init; }
        /// <summary>Gets the number of authentication requests allowed to wait for a rate-limit permit.</summary>
        public int AuthenticationQueueLimit { get; init; }
    }

    /// <summary>Contains optional external authentication-provider credentials.</summary>
    internal sealed class ExternalProviderOptions
    {
        /// <summary>Gets the Google OAuth credentials.</summary>
        public ExternalProviderCredentials Google { get; init; } = new();
        /// <summary>Gets the Microsoft OAuth credentials.</summary>
        public ExternalProviderCredentials Microsoft { get; init; } = new();
    }

    /// <summary>Contains the client credentials for one external authentication provider.</summary>
    internal sealed class ExternalProviderCredentials
    {
        /// <summary>Gets the provider-issued OAuth client identifier.</summary>
        public string ClientId { get; init; } = string.Empty;
        /// <summary>Gets the provider-issued OAuth client secret.</summary>
        public string ClientSecret { get; init; } = string.Empty;
    }
}
