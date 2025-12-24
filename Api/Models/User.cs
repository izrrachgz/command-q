namespace Api.Models
{
    public sealed class User
    {
        public Guid Id { get; init; }
        public string Username { get; init; } = null!;
        public string PasswordHash { get; init; } = null!;
        public int TokenVersion { get; set; }
        public List<string> Roles { get; init; } = new();

        public int FailedLoginAttempts { get; set; }
        public DateTime? LockedUntilUtc { get; set; }
    }
}
