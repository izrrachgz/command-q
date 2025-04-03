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

        public PgSqlCommandRunner(IConfiguration configuration) : this(configuration.GetDatabaseConnection())
        { }

        public async Task<IDbConnection> GetConnection(CancellationToken cancellationToken = default)
        {
            var connection = new NpgsqlConnection(ConnectionString);

            await connection.OpenAsync(cancellationToken);

            return connection;
        }

        public async Task<List<Task<List<object>>>> RunInParallel(List<QueryCommand> commands, CancellationToken cancellationToken = default)
        {
            var tasks = new List<Task<List<object>>>(commands.Count);

            foreach (var command in commands)
            {
                tasks.Add(Execute(command.Query, command.ReturnType, command.Parameters, command.CancellationToken));
            }

            await Parallel.ForEachAsync(tasks, cancellationToken, async (t, token) => { await t.WaitAsync(token); });

            return tasks;
        }

        public async Task<List<object>> Execute(string query, Type type, Dictionary<string, object?>? parameters, CancellationToken cancellationToken = default)
        {
            List<object> results = [];

            try
            {
                await using var connection = (NpgsqlConnection)await GetConnection(cancellationToken);

                await using var cmd = connection.CreateCommand();

                cmd.CommandText = query;

                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        object value = parameter.Value is null ? DBNull.Value : parameter.Value;
                        cmd.Parameters.AddWithValue(parameter.Key, value);
                    }
                }

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

                if (reader.HasRows)
                    results = [.. reader.Parse(type)];
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine(CommandConstants.CommandCancelled);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return results;
        }

        public async Task<BasicResponse> BulkInsert(string table, string[] columns, Dictionary<string, object?>[] rows, CancellationToken cancellationToken = default)
        {
            string joinedColumns = $@"""{string.Join(@""",""", columns)}""";

            ReadOnlyCollection<NpgsqlDbColumn> schema;

            await using var connection = (NpgsqlConnection)await GetConnection(cancellationToken);

            try
            {
                await using var cmd = connection.CreateCommand();
                cmd.CommandText = $@"SELECT {joinedColumns} FROM ""{table}"" LIMIT 0";

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

                schema = await reader.GetColumnSchemaAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return new(false, CommandConstants.CommandCancelled);
            }
            catch
            {
                return new(false, CommandConstants.CannotReadSchema);
            }

            try
            {
                await using var writer = await connection.BeginBinaryImportAsync($@"COPY ""{table}"" ({joinedColumns}) FROM STDIN BINARY", cancellationToken);

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

                return new(true, CommandConstants.CopyCompleted);
            }
            catch (OperationCanceledException)
            {
                return new(false, CommandConstants.TransactionCancelled);
            }
            catch
            {
                return new(false, CommandConstants.ErrorDuringCopy);
            }
        }
    }
}
