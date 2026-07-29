#nullable enable
namespace Domain.Interfaces
{
    /// <summary>
    /// Represents a persisted entity with lifecycle timestamps.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Primary identifier associated with the entity.
        /// </summary>
        long Id { get; set; }

        /// <summary>
        /// Date and time when the entity was created.
        /// </summary>
        DateTime Created { get; set; }

        /// <summary>
        /// Date and time when the entity was last modified.
        /// </summary>
        DateTime Modified { get; set; }

        /// <summary>
        /// Date and time when the entity was soft-deleted, when applicable.
        /// </summary>
        DateTime? Deleted { get; set; }
    }
}
