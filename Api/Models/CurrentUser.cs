namespace Api.Models
{
    /// <summary>
    /// Authenticated account information resolved from the current claims principal.
    /// </summary>
    public sealed record CurrentUser
    {
        public Guid AccountId { get; init; }

        public string? Username { get; init; }

        public string? JwtId { get; init; }

        public IReadOnlyList<string> Roles { get; init; } = [];

        public bool IsAuthenticated { get; init; }
    }
}
