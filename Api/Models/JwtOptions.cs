namespace Api.Models
{
    public sealed class JwtOptions
    {
        public string SecretKey { get; init; } = null!;
        public string Issuer { get; init; } = null!;
        public string Audience { get; init; } = null!;
        public int MinutesToExpire { get; init; }
        public int ClockSkewMinutes { get; init; }
        public int RefreshTokenDays { get; init; }
    }

}
