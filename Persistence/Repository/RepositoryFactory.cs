using Npgsql;
using System.Data;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data.SQLite;
using Common.Enumerations;
using MySql.Data.MySqlClient;
using Microsoft.Data.SqlClient;

namespace Persistence.Repository
{
    internal static class RepositoryFactory
    {
        public static QueryFactory CreateQueryFactory(string connectionString, DatabaseEngine engine)
        {
            var (connection, compiler) = CreateConnection(engine, connectionString);

            return new QueryFactory(connection, compiler);
        }

        public static string GetMigrationsPath(DatabaseEngine engine, string? configuredRootPath = null, string? contentRootPath = null)
        {
            string migrationsRootPath = ResolveMigrationsRootPath(configuredRootPath, contentRootPath);

            return Path.Combine(migrationsRootPath, engine.ToString());
        }

        private static (IDbConnection Connection, Compiler Compiler) CreateConnection(DatabaseEngine engine, string connectionString)
        {
            return engine switch
            {
                DatabaseEngine.MsSql => (new SqlConnection(connectionString), new SqlServerCompiler()),
                DatabaseEngine.MySql => (new MySqlConnection(connectionString), new MySqlCompiler()),
                DatabaseEngine.SqLite => (new SQLiteConnection(connectionString), new SqliteCompiler()),
                DatabaseEngine.PgSql => (new NpgsqlConnection(connectionString), new PostgresCompiler()),
                _ => (new NpgsqlConnection(connectionString), new PostgresCompiler())
            };
        }

        private static string ResolveMigrationsRootPath(string? configuredRootPath, string? contentRootPath)
        {
            if (!string.IsNullOrWhiteSpace(configuredRootPath))
            {
                return Path.GetFullPath(configuredRootPath, contentRootPath ?? Directory.GetCurrentDirectory());
            }

            string[] candidates =
            [
                Path.Combine(contentRootPath ?? string.Empty, nameof(Persistence), nameof(Migrations)),
                Path.Combine(contentRootPath ?? string.Empty, "..", nameof(Persistence), nameof(Migrations)),
                Path.Combine(AppContext.BaseDirectory, nameof(Persistence), nameof(Migrations)),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", nameof(Persistence), nameof(Migrations)),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", nameof(Persistence), nameof(Migrations)),
                Path.Combine(Directory.GetCurrentDirectory(), nameof(Persistence), nameof(Migrations)),
                Path.Combine(Directory.GetCurrentDirectory(), "..", nameof(Persistence), nameof(Migrations)),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", nameof(Persistence), nameof(Migrations)),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", nameof(Persistence), nameof(Migrations))
            ];

            return candidates
                .Select(Path.GetFullPath)
                .FirstOrDefault(Directory.Exists) ??
                Path.GetFullPath(Path.Combine(contentRootPath ?? Directory.GetCurrentDirectory(), "..", nameof(Persistence), nameof(Migrations)));
        }
    }
}
