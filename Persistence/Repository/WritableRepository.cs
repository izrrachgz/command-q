using SqlKata;
using SqlKata.Execution;
using Domain.Interfaces;
using Common.Enumerations;
using Microsoft.Extensions.Logging;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository
{
    /// <summary>
    /// SqlKata-backed repository that exposes read operations plus the write-capable query factory.
    /// </summary>
    public class WritableRepository : IWritableRepository
    {
        /// <summary>
        /// Database query entry point with write-capable SqlKata operations.
        /// </summary>
        public QueryFactory Db { get; }

        /// <summary>
        /// Database engine associated with this repository instance.
        /// </summary>
        public DatabaseEngine Engine { get; }

        /// <summary>
        /// Folder containing SQL migration scripts for the repository engine.
        /// </summary>
        public string MigrationsPath { get; }

        /// <summary>
        /// Creates a writable repository for a specific connection string and database engine.
        /// </summary>
        public WritableRepository(string connectionString, DatabaseEngine engine, ILogger<WritableRepository> logger)
        {
            Engine = engine;
            MigrationsPath = RepositoryFactory.GetMigrationsPath(Engine);
            Db = RepositoryFactory.CreateQueryFactory(connectionString, Engine);

            logger.LogDebug(@"Repository instance created, using engine : {Engine}", Engine);
        }

        /// <summary>
        /// Creates a writable repository from the configured database connection and engine.
        /// </summary>
        public WritableRepository(IConfiguration configuration, ILogger<WritableRepository> logger) : this(configuration.GetDatabaseConnection(), configuration.GetDatabaseEngine(), logger)
        { }

        /// <summary>
        /// Creates a SqlKata query for the specified table.
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
            return await Db.FirstOrDefaultAsync<T>(query, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Executes a query and returns all matching rows.
        /// </summary>
        public async Task<IEnumerable<T>> GetAsync<T>(Query query, CancellationToken cancellationToken = default)
        {
            return await Db.GetAsync<T>(query, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Executes a query and returns whether any matching row exists.
        /// </summary>
        public async Task<bool> ExistsAsync(Query query, CancellationToken cancellationToken = default)
        {
            return await Db.ExistsAsync(query, cancellationToken: cancellationToken);
        }
    }
}
