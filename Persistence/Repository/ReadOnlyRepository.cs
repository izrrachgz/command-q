using SqlKata;
using Domain.Interfaces;
using SqlKata.Execution;
using Common.Enumerations;
using Microsoft.Extensions.Logging;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository
{
    public class ReadOnlyRepository : IReadOnlyRepository
    {
        private readonly QueryFactory _db;

        public DatabaseEngine Engine { get; }

        public ReadOnlyRepository(string connectionString, DatabaseEngine engine, ILogger<ReadOnlyRepository> logger)
        {
            Engine = engine;
            _db = RepositoryFactory.CreateQueryFactory(connectionString, Engine);

            logger.LogDebug(@"Readonly repository instance created, using engine : {Engine}", Engine);
        }

        public ReadOnlyRepository(IConfiguration configuration, ILogger<ReadOnlyRepository> logger) : this(configuration.GetDatabaseConnection(), configuration.GetDatabaseEngine(), logger)
        { }

        public Query Query(string table)
        {
            return new Query(table);
        }

        public async Task<T?> FirstOrDefaultAsync<T>(Query query, CancellationToken cancellationToken = default)
        {
            return await _db.FirstOrDefaultAsync<T>(query, cancellationToken: cancellationToken);
        }

        public async Task<IEnumerable<T>> GetAsync<T>(Query query, CancellationToken cancellationToken = default)
        {
            return await _db.GetAsync<T>(query, cancellationToken: cancellationToken);
        }

        public async Task<bool> ExistsAsync(Query query, CancellationToken cancellationToken = default)
        {
            return await _db.ExistsAsync(query, cancellationToken: cancellationToken);
        }
    }
}
