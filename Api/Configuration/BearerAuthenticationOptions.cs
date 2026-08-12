namespace Api.Configuration
{
    internal sealed class BearerAuthenticationOptions
    {
        public string Authority { get; init; } = string.Empty;
        public string Audience { get; init; } = string.Empty;
        public string ApiScope { get; init; } = string.Empty;
        public bool RequireHttpsMetadata { get; init; }
        public int ClockSkewSeconds { get; init; }
    }

}
