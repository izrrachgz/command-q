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
    public class WritableRepository : IWritableRepository
    {
        public QueryFactory Db { get; }

        public DatabaseEngine Engine { get; }

        public string MigrationsPath { get; }

        public WritableRepository(string connectionString, DatabaseEngine engine)
        {
            MigrationsPath = $@"{Directory.GetParent(Directory.GetCurrentDirectory())}\{nameof(Persistence)}\{nameof(Migrations)}\";

            Engine = engine;

            IDbConnection connection;
            Compiler compiler;

            switch (Engine)
            {
                case DatabaseEngine.MsSql:
                    connection = new SqlConnection(connectionString);
                    compiler = new SqlServerCompiler();
                    MigrationsPath += nameof(DatabaseEngine.MsSql);
                    break;
                case DatabaseEngine.MySql:
                    connection = new MySqlConnection(connectionString);
                    compiler = new MySqlCompiler();
                    MigrationsPath += nameof(DatabaseEngine.MySql);
                    break;
                case DatabaseEngine.SqLite:
                    connection = new SQLiteConnection(connectionString);
                    compiler = new SqliteCompiler();
                    MigrationsPath += nameof(DatabaseEngine.SqLite);
                    break;
                case DatabaseEngine.PgSql:
                default:
                    connection = new NpgsqlConnection(connectionString);
                    compiler = new PostgresCompiler();
                    MigrationsPath += nameof(DatabaseEngine.PgSql);
                    break;
            }

            MigrationsPath += '/';

            Db = new QueryFactory(connection, compiler);
        }

        public WritableRepository(IConfiguration configuration) : this(GetConnectionString(configuration), GetDatabaseEngine(configuration))
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
