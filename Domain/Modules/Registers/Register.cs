using Domain.Entities;

namespace Domain.Modules.Registers
{
    public class Register : BaseEntity
    {
        public required Guid AccountId { get; set; }
        public required TrackingAction Action { get; set; }
    }
}
