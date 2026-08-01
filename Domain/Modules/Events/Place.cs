using Domain.Entities;

namespace Domain.Modules.Events
{
    public class Place : BaseEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}
