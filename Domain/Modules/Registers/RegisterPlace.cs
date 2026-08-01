using Domain.Entities;

namespace Domain.Modules.Registers
{
    public class RegisterPlace : BaseEntity
    {
        public required long RegisterId { get; set; }
        public required long PlaceId { get; set; }
    }
}
