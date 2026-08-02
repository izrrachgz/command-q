namespace Api.Models
{
    public sealed class BearerAuthenticationOptions
    {
        public string Authority { get; init; } = string.Empty;
        public string Audience { get; init; } = string.Empty;
        public string ApiScope { get; init; } = string.Empty;
        public bool RequireHttpsMetadata { get; init; }
        public int ClockSkewSeconds { get; init; }
    }

    public static class AuthenticationClaimTypes
    {
        public const string AccessTokenType = "at+jwt";
        public const string Email = "email";
        public const string Issuer = "iss";
        public const string JwtId = "jti";
        public const string Name = "name";
        public const string PreferredUsername = "preferred_username";
        public const string Role = "role";
        public const string Scope = "scope";
        public const string Subject = "sub";
    }
}
