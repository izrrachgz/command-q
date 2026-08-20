using Domain.Abstractions;

namespace Domain.Entities
{
    public class BaseEntity<TKey> : IEntity<TKey>, IAuditableEntity, ISoftDeletableEntity
    {
        public TKey Id { get; set; } = default!;

        /// <summary>Gets or sets when the entity was created, in UTC.</summary>
        public DateTime Created { get; set; }

        /// <summary>Gets or sets when the entity was last modified, in UTC.</summary>
        public DateTime Modified { get; set; }

        /// <summary>Gets or sets when the entity was soft-deleted, in UTC.</summary>
        public DateTime? Deleted { get; set; }
    }

    public class BaseEntity : BaseEntity<long>
    {
    }
}
