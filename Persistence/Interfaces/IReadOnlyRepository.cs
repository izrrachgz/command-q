using SqlKata;
using Common.Enumerations;

namespace Persistence.Interfaces
{
    /// <summary>
    /// Provides a database entry point intended for read operations without exposing write-capable SqlKata APIs.
    /// </summary>
    public interface IReadOnlyRepository
    {
        /// <summary>
        /// Database engine associated with this repository instance.
        /// </summary>
        DatabaseEngine Engine { get; }

        /// <summary>
        /// Creates a read query for the specified table.
        /// </summary>
        /// <param name="table">Table to query.</param>
        Query Query(string table);

        /// <summary>
        /// Executes a query and returns the first matching row, or null when no row exists.
        /// </summary>
        /// <param name="query">SqlKata query to execute.</param>
        /// <param name="cancellationToken">Token used while executing the query.</param>
        Task<T?> FirstOrDefaultAsync<T>(Query query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a query and returns all matching rows.
        /// </summary>
        /// <param name="query">SqlKata query to execute.</param>
        /// <param name="cancellationToken">Token used while executing the query.</param>
        Task<IEnumerable<T>> GetAsync<T>(Query query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a query and returns whether any matching row exists.
        /// </summary>
        /// <param name="query">SqlKata query to execute.</param>
        /// <param name="cancellationToken">Token used while executing the query.</param>
        Task<bool> ExistsAsync(Query query, CancellationToken cancellationToken = default);
    }
}
