using Domain.Entities;

namespace Domain.Modules.Events
{
    public class Event : BaseEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        /// <summary>Gets or sets the event start instant, in UTC.</summary>
        public required DateTime StartUtc { get; set; }
        /// <summary>Gets or sets the event end instant, in UTC.</summary>
        public required DateTime EndUtc { get; set; }
    }
}
