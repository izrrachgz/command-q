using Npgsql;
using System.Data;
using Domain.Interfaces;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data.SQLite;
using Common.Enumerations;
using MySql.Data.MySqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository
{
    public class ReadOnlyRepository : IReadOnlyRepository
    {
        public QueryFactory Db => _db;

        public DatabaseEngine Engine { get; }

        private readonly QueryFactory _db;

        public ReadOnlyRepository(string connectionString, DatabaseEngine engine, ILogger<ReadOnlyRepository> logger)
        {
            Engine = engine;

            IDbConnection connection;
            Compiler compiler;

            switch (Engine)
            {
                case DatabaseEngine.MsSql:
                    connection = new SqlConnection(connectionString);
                    compiler = new SqlServerCompiler();
                    break;
                case DatabaseEngine.MySql:
                    connection = new MySqlConnection(connectionString);
                    compiler = new MySqlCompiler();
                    break;
                case DatabaseEngine.SqLite:
                    connection = new SQLiteConnection(connectionString);
                    compiler = new SqliteCompiler();
                    break;
                case DatabaseEngine.PgSql:
                default:
                    connection = new NpgsqlConnection(connectionString);
                    compiler = new PostgresCompiler();
                    break;
            }

            _db = new(connection, compiler);

            logger.LogDebug("Readonly repository instance created, using engine : {Engine}", Engine);
        }

        public ReadOnlyRepository(IConfiguration configuration, ILogger<ReadOnlyRepository> logger) : this(configuration.GetDatabaseConnection(), configuration.GetDatabaseEngine(), logger)
        { }
    }
}
