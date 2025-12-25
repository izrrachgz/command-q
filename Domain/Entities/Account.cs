namespace Domain.Entities
{
    public class Account : BaseEntity
    {
        public new required Guid Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public int TokenVersion { get; set; }
        public string[] Roles { get; init; } = [];
        public byte FailedLoginAttempts { get; set; }
        public DateTime? LockedUntilUtc { get; set; }
    }
}
