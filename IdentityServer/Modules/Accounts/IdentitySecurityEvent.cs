namespace IdentityServer.Modules.Accounts
{
    public sealed class IdentitySecurityEvent
    {
        public long Id { get; set; }
        public Guid? UserId { get; set; }
        public required string Action { get; set; }
        public string? ClientId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? DetailsJson { get; set; }
        public DateTime OccurredUtc { get; set; }
    }
}
