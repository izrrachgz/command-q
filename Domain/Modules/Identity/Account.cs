using Domain.Entities;

namespace Domain.Modules.Identity
{
    public class Account : BaseEntity<Guid>
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
    }
}
