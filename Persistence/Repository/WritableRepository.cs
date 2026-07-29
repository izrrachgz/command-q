using SqlKata;
using SqlKata.Execution;
using Domain.Interfaces;
using Common.Enumerations;
using Microsoft.Extensions.Logging;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository
{
    public class WritableRepository : IWritableRepository
    {
        public QueryFactory Db { get; }

        public DatabaseEngine Engine { get; }

        public string MigrationsPath { get; }

        public WritableRepository(string connectionString, DatabaseEngine engine, ILogger<WritableRepository> logger)
        {
            Engine = engine;
            MigrationsPath = RepositoryFactory.GetMigrationsPath(Engine);
            Db = RepositoryFactory.CreateQueryFactory(connectionString, Engine);

            logger.LogDebug(@"Repository instance created, using engine : {Engine}", Engine);
        }

        public WritableRepository(IConfiguration configuration, ILogger<WritableRepository> logger) : this(configuration.GetDatabaseConnection(), configuration.GetDatabaseEngine(), logger)
        { }

        public Query Query(string table)
        {
            return new Query(table);
        }

        public async Task<T?> FirstOrDefaultAsync<T>(Query query, CancellationToken cancellationToken = default)
        {
            return await Db.FirstOrDefaultAsync<T>(query, cancellationToken: cancellationToken);
        }

        public async Task<IEnumerable<T>> GetAsync<T>(Query query, CancellationToken cancellationToken = default)
        {
            return await Db.GetAsync<T>(query, cancellationToken: cancellationToken);
        }

        public async Task<bool> ExistsAsync(Query query, CancellationToken cancellationToken = default)
        {
            return await Db.ExistsAsync(query, cancellationToken: cancellationToken);
        }
    }
}
