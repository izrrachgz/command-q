#nullable enable
namespace Domain.Interfaces
{
    /// <summary>
    /// Should represent an entity object
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Should store the primary identifier asociated
        /// </summary>
        long Id { get; set; }

        /// <summary>
        /// Should store the time when the entity has been created
        /// </summary>
        DateTime Created { get; set; }

        /// <summary>
        /// Should store the time when the entity has been modified
        /// </summary>
        DateTime Modified { get; set; }

        /// <summary>
        /// Should store the time when the entity has been marked as deleted
        /// </summary>
        DateTime? Deleted { get; set; }
    }
}
