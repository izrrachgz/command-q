using Npgsql;
using System.Data;
using SqlKata.Compilers;
using SqlKata.Execution;
using Domain.Interfaces;
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

        public ReadOnlyRepository(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString(ApplicationConfiguration.ConnectionStringKeyName) ?? string.Empty;

            Engine = configuration.GetValue<DatabaseEngine>(ApplicationConfiguration.EngineKeyName);

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
                case DatabaseEngine.PgSql:
                default:
                    connection = new NpgsqlConnection(connectionString);
                    compiler = new PostgresCompiler();
                    break;
            }

            _db = new(connection, compiler);
        }
    }
}
