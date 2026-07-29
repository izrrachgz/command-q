using SqlKata;
using Persistence.Interfaces;
using SqlKata.Execution;
using Common.Enumerations;
using Microsoft.Extensions.Logging;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository
{
    /// <summary>
    /// SqlKata-backed repository that exposes only read operations to application query handlers.
    /// </summary>
    public class ReadOnlyRepository : IReadOnlyRepository
    {
        private readonly QueryFactory _db;

        public DatabaseEngine Engine { get; }

        /// <summary>
        /// Creates a read-only repository for a specific connection string and database engine.
        /// </summary>
        public ReadOnlyRepository(string connectionString, DatabaseEngine engine, ILogger<ReadOnlyRepository> logger)
        {
            Engine = engine;
            _db = RepositoryFactory.CreateQueryFactory(connectionString, Engine);

            logger.LogDebug(@"Readonly repository instance created, using engine : {Engine}", Engine);
        }

        /// <summary>
        /// Creates a read-only repository from the configured database connection and engine.
        /// </summary>
        public ReadOnlyRepository(IConfiguration configuration, ILogger<ReadOnlyRepository> logger) : this(configuration.GetDatabaseConnection(), configuration.GetDatabaseEngine(), logger)
        { }

        /// <summary>
        /// Creates a SqlKata query for the specified table without exposing the underlying write-capable query factory.
        /// </summary>
        public Query Query(string table)
        {
            return new Query(table);
        }

        /// <summary>
        /// Executes a query and returns the first matching row, or null when no row exists.
        /// </summary>
        public async Task<T?> FirstOrDefaultAsync<T>(Query query, CancellationToken cancellationToken = default)
        {
            return await _db.FirstOrDefaultAsync<T>(query, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Executes a query and returns all matching rows.
        /// </summary>
        public async Task<IEnumerable<T>> GetAsync<T>(Query query, CancellationToken cancellationToken = default)
        {
            return await _db.GetAsync<T>(query, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Executes a query and returns whether any matching row exists.
        /// </summary>
        public async Task<bool> ExistsAsync(Query query, CancellationToken cancellationToken = default)
        {
            return await _db.ExistsAsync(query, cancellationToken: cancellationToken);
        }
    }
}
