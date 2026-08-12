using Domain.Abstractions;

namespace Domain.Entities
{
    public class BaseEntity<TKey> : IEntity<TKey>, IAuditableEntity, ISoftDeletableEntity
    {
        public TKey Id { get; set; } = default!;

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }

        public DateTime? Deleted { get; set; }
    }

    public class BaseEntity : BaseEntity<long>
    {
    }
}
