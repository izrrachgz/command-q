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

        public static string GetMigrationsPath(DatabaseEngine engine)
        {
            var path = $@"{Directory.GetParent(Directory.GetCurrentDirectory())}\{nameof(Persistence)}\{nameof(Migrations)}\";

            return $@"{path}{engine}/";
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
    }
}
