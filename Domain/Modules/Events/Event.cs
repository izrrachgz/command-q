using Domain.Entities;

namespace Domain.Modules.Events
{
    public class Event : BaseEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required DateTime Start { get; set; }
        public required DateTime End { get; set; }
    }
}
