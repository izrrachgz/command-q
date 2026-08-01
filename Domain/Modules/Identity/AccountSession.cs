using Domain.Entities;

namespace Domain.Modules.Identity
{
    public class AccountSession
    {
        public required Guid AccountId { get; set; }
        public int TokenVersion { get; set; }
        public byte FailedLoginAttempts { get; set; }
        public DateTime? LockedUntilUtc { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime? Deleted { get; set; }
    }
}
