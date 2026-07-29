using SqlKata;
using Common.Enumerations;

namespace Domain.Interfaces
{
    /// <summary>
    /// Provides a database entry point intended for read operations.
    /// </summary>
    public interface IReadOnlyRepository
    {
        /// <summary>
        /// Associated database engine.
        /// </summary>
        DatabaseEngine Engine { get; }

        /// <summary>
        /// Creates a read query for the specified table.
        /// </summary>
        Query Query(string table);

        /// <summary>
        /// Executes a query and returns the first matching row, or null when no row exists.
        /// </summary>
        Task<T?> FirstOrDefaultAsync<T>(Query query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a query and returns all matching rows.
        /// </summary>
        Task<IEnumerable<T>> GetAsync<T>(Query query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a query and returns whether any matching row exists.
        /// </summary>
        Task<bool> ExistsAsync(Query query, CancellationToken cancellationToken = default);
    }
}
