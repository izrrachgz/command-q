using Npgsql;
using SqlKata.Compilers;
using SqlKata.Execution;
using Domain.Interfaces;
using Application.Constants;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository
{
    public class WritableRepository : IRepository
    {
        public QueryFactory Db { get; }

        public WritableRepository(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString(ApplicationConfiguration.ConnectionStringKeyName) ?? string.Empty;
            NpgsqlConnection connection = new(connectionString);
            PostgresCompiler compiler = new();
            Db = new(connection, compiler);
        }
    }
}
