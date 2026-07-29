namespace Api.Models
{
    public sealed class AuthOptions
    {
        public int MaxFailedAttempts { get; init; }

        public int LockoutMinutes { get; init; }

        public int RefreshTokenByteSize { get; init; }
    }
}
