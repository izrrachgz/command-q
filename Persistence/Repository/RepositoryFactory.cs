using Dapper;
using Npgsql;
using System.Data;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data.SQLite;
using Common.Enumerations;
using MySql.Data.MySqlClient;
using Microsoft.Data.SqlClient;
using Persistence.Repository.TypeHandlers;

namespace Persistence.Repository
{
    internal static class RepositoryFactory
    {
        public static QueryFactory CreateQueryFactory(string connectionString, DatabaseEngine engine)
        {
            var (connection, compiler) = CreateConnection(engine, connectionString);

            return new QueryFactory(connection, compiler);
        }

        private static (IDbConnection Connection, Compiler Compiler) CreateConnection(DatabaseEngine engine, string connectionString)
        {
            if (engine == DatabaseEngine.SqLite)
            {
                SqlMapper.AddTypeHandler(SqliteGuidTypeHandler.Instance);
                SqlMapper.AddTypeHandler(SqliteUtcDateTimeTypeHandler.Instance);
            }

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
