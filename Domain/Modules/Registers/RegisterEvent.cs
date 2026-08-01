using Domain.Entities;

namespace Domain.Modules.Registers
{
    public class RegisterEvent : BaseEntity
    {
        public required long RegisterId { get; set; }
        public required long EventId { get; set; }
    }
}
