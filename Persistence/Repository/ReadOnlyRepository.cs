using Npgsql;
using System.Data;
using SqlKata.Compilers;
using SqlKata.Execution;
using Domain.Interfaces;
using System.Data.SQLite;
using Common.Enumerations;
using Application.Constants;
using MySql.Data.MySqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository
{
    public class ReadOnlyRepository : IReadOnlyRepository
    {
        public QueryFactory Db => _db;

        public DatabaseEngine Engine { get; }

        private readonly QueryFactory _db;

        public ReadOnlyRepository(string connectionString, DatabaseEngine engine)
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
        }

        public ReadOnlyRepository(IConfiguration configuration) : this(GetConnectionString(configuration), GetDatabaseEngine(configuration))
        { }

        private static string GetConnectionString(IConfiguration configuration)
        {
            return configuration.GetConnectionString(ApplicationConfiguration.ConnectionStringKeyName) ?? string.Empty;
        }

        private static DatabaseEngine GetDatabaseEngine(IConfiguration configuration)
        {
            return configuration.GetValue<DatabaseEngine>(ApplicationConfiguration.EngineKeyName);
        }
    }
}
