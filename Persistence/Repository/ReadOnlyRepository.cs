using Npgsql;
using SqlKata.Compilers;
using SqlKata.Execution;
using Domain.Interfaces;
using Application.Constants;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository
{
    public class ReadOnlyRepository : IReadOnlyRepository
    {
        public QueryFactory Db => _db;
        private readonly QueryFactory _db;

        public ReadOnlyRepository(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString(ApplicationConfiguration.ConnectionStringKeyName) ?? string.Empty;
            NpgsqlConnection connection = new(connectionString);
            PostgresCompiler compiler = new();
            _db = new(connection, compiler);
        }
    }
}
