namespace Application.AccountSessions.Dtos
{
    public sealed record AccountSessionModel
    {
        public Guid AccountId { get; set; }
        public Guid SessionId { get; set; } = Guid.NewGuid();
        public string UniqueName { get; set; } = Guid.NewGuid().ToString();
        public string[] Roles { get; set; } = [];
        public int TokenVersion { get; set; }
        public byte FailedLoginAttempts { get; set; }
        public DateTime? LockedUntilUtc { get; set; }
    }
}
