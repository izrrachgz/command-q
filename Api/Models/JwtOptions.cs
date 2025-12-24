namespace Api.Models
{
    public sealed class JwtOptions
    {
        public string SecretKey { get; init; } = null!;
        public string Issuer { get; init; } = null!;
        public string Audience { get; init; } = null!;
        public int AccessTokenMinutes { get; init; }
        public int RefreshTokenDays { get; init; }
    }

}
