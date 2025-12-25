namespace Api.Models
{
    public sealed class RefreshToken
    {
        public string Token { get; init; } = null!;
        public Guid AccountId { get; init; }
        public DateTime ExpiresAt { get; init; }
        public bool Revoked { get; set; }
    }
}
