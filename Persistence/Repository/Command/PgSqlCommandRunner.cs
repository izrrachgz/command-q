using Npgsql;
using Dapper;
using System.Data;
using Npgsql.Schema;
using Domain.Interfaces;
using Persistence.Constant;
using Common.Models.Command;
using Common.Models.Response;
using System.Collections.ObjectModel;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository.Command
{
    internal sealed class PgSqlCommandRunner : ICommandRunner
    {
        private string ConnectionString { get; }

        public PgSqlCommandRunner(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public PgSqlCommandRunner(IConfiguration configuration) : this(configuration.GetConnectionString())
        { }

        public IDbConnection GetConnection()
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(ConnectionString);
            dataSourceBuilder.UseVector();

            var dataSource = dataSourceBuilder.Build();
            return dataSource.OpenConnection();
        }

        public async Task<List<Task<List<object>>>> RunInParallel(List<QueryCommand> commands)
        {
            var tasks = new List<Task<List<object>>>(commands.Count);

            foreach (var command in commands)
            {
                tasks.Add(Execute(command.Query, command.ReturnType, command.Parameters, command.CancellationToken));
            }

            await Parallel.ForEachAsync(tasks, async (t, token) => { await t.WaitAsync(token); });

            return tasks;
        }

        public async Task<List<object>> Execute(string query, Type type, Dictionary<string, object?>? parameters, CancellationToken cancellationToken = default)
        {
            List<object> results = [];

            var connection = GetConnection();

            using var cmd = (NpgsqlCommand)connection.CreateCommand();

            cmd.CommandText = query;

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    object value = parameter.Value is null ? DBNull.Value : parameter.Value;
                    cmd.Parameters.AddWithValue(parameter.Key, value);
                }
            }

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (reader.HasRows)
                results = [.. reader.Parse(type)];

            return results;
        }

        public async Task<BasicResponse> BulkInsert(string table, string[] columns, Dictionary<string, object?>[] rows, CancellationToken cancellationToken = default)
        {
            var connection = (NpgsqlConnection)GetConnection();

            string joinedColumns = $@"""{string.Join(@""",""", columns)}""";
            ReadOnlyCollection<NpgsqlDbColumn> schema;
            try
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = $@"SELECT {joinedColumns} FROM ""{table}"" LIMIT 0";

                using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                schema = await reader.GetColumnSchemaAsync(cancellationToken);

                await cmd.DisposeAsync();
                await reader.DisposeAsync();
            }
            catch
            {
                await connection.DisposeAsync();

                return new(false, CommandConstants.CannotReadSchema);
            }

            using var writer = await connection.BeginBinaryImportAsync($@"COPY ""{table}"" ({joinedColumns}) FROM STDIN BINARY", cancellationToken);
            try
            {
                foreach (var row in rows)
                {
                    await writer.StartRowAsync(cancellationToken);
                    foreach (var item in row)
                    {
                        if (item.Value == null)
                            await writer.WriteNullAsync(cancellationToken);

                        var type = schema.FirstOrDefault(s => s.ColumnName == item.Key);

                        if (type is not null && type.NpgsqlDbType is not null)
                        {
                            await writer.WriteAsync(item.Value, type.NpgsqlDbType.Value, cancellationToken);
                            continue;
                        }

                        if (type is not null && type.NpgsqlDbType is null && type.DataTypeName.Equals("public.vector"))
                        {
                            await writer.WriteAsync(item.Value, "vector", cancellationToken);
                        }
                    }
                }
                await writer.CompleteAsync(cancellationToken);

                await connection.DisposeAsync();

                return new(true, CommandConstants.CopyCompleted);
            }
            catch
            {
                return new(false, CommandConstants.ErrorDuringCopy);
            }
        }
    }
}
