using Domain.Entities;

namespace Domain.Modules.Identity
{
    public sealed class AccountIdentity : BaseEntity
    {
        public required Guid AccountId { get; set; }
        public required string Issuer { get; set; }
        public required string Subject { get; set; }
    }
}
