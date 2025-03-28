using Domain.Interfaces;

namespace Domain.Entities
{
    public class BaseEntity : IEntity
    {
        public long Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime? Deleted { get; set; }
    }
}
