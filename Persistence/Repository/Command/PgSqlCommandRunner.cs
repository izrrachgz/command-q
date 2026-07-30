using Npgsql;
using Dapper;
using System.Data;
using Npgsql.Schema;
using Common.Constants;
using Persistence.Constant;
using Common.Models.Command;
using Persistence.Interfaces;
using Common.Models.Response;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository.Command
{
    /// <summary>
    /// Executes commands and bulk inserts against PostgreSQL.
    /// </summary>
    public class PgSqlCommandRunner : ICommandRunner
    {
        public ILogger Logger { get; set; }

        private string ConnectionString { get; }

        /// <summary>
        /// Creates a PostgreSQL command runner for a specific connection string.
        /// </summary>
        public PgSqlCommandRunner(string connectionString, ILogger<PgSqlCommandRunner> logger)
        {
            Logger = logger;
            ConnectionString = connectionString;

            Logger.LogDebug(InstanceConstants.InstanceCreated, nameof(PgSqlCommandRunner));
        }

        /// <summary>
        /// Creates a PostgreSQL command runner from the configured database connection.
        /// </summary>
        public PgSqlCommandRunner(IConfiguration configuration, ILogger<PgSqlCommandRunner> logger) : this(configuration.GetDatabaseConnection(), logger)
        { }

        public async Task<IDbConnection> GetConnection(CancellationToken cancellationToken = default)
        {
            var connection = new NpgsqlConnection(ConnectionString);

            await connection.OpenAsync(cancellationToken);

            return connection;
        }

        public async Task<List<List<object>>> ExecuteCommands(List<QueryCommand> commands, CancellationToken cancellationToken = default)
        {
            var tasks = commands
                .Select(c => Execute(c.Query, c.ReturnType, c.Parameters, c.CancellationToken))
                .ToList();

            Logger.LogDebug(ProcessConstants.ExecutingCommands, commands.Count);

            try
            {
                var results = await Task.WhenAll(tasks);
                return [.. results];
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine(CommandConstants.CommandCancelled);

                Logger.LogDebug(CommandConstants.CommandCancelled);
            }
            catch (Exception e)
            {
                Console.WriteLine($@"{ErrorConstants.AnExceptionHasOccurred}: {e.Message}");

                Logger.LogError(e, ErrorConstants.AnExceptionHasOccurred);
            }

            return [];
        }

        public async Task<List<object>> Execute(string query, Type type, Dictionary<string, object?>? parameters, CancellationToken cancellationToken = default)
        {
            List<object> results = [];

            try
            {
                await using var connection = (NpgsqlConnection)await GetConnection(cancellationToken);

                await using var cmd = connection.CreateCommand();

                cmd.CommandText = query;

                Logger.LogDebug(DatabaseConstants.ExecutingQuery, query);

                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        object value = parameter.Value is null ? DBNull.Value : parameter.Value;
                        cmd.Parameters.AddWithValue(parameter.Key, value);

                        Logger.LogDebug(DatabaseConstants.ParameterInQuery, parameter.Key, parameter.Value?.GetType().Name ?? nameof(DBNull));
                    }
                }

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

                if (reader.HasRows)
                    results = [.. reader.Parse(type)];
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine(CommandConstants.CommandCancelled);

                Logger.LogDebug(CommandConstants.CommandCancelled);
            }
            catch (Exception e)
            {
                Console.WriteLine($@"{ErrorConstants.AnExceptionHasOccurred}: {e.Message}");

                Logger.LogError(e, ErrorConstants.AnExceptionHasOccurred);
            }

            return results;
        }

        public async Task<BasicResponse> BulkInsert(string table, string[] columns, Dictionary<string, object?>[] rows, CancellationToken cancellationToken = default)
        {
            Logger.LogDebug(DatabaseConstants.SavingRecords, rows.Length);

            string joinedColumns = $@"""{string.Join(@""",""", columns)}""";

            ReadOnlyCollection<NpgsqlDbColumn> schema;

            await using var connection = (NpgsqlConnection)await GetConnection(cancellationToken);

            try
            {
                await using var cmd = connection.CreateCommand();
                cmd.CommandText = $@"SELECT {joinedColumns} FROM ""{table}"" LIMIT 0";

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

                schema = reader.GetColumnSchema();
            }
            catch (OperationCanceledException)
            {
                Logger.LogDebug(ProcessConstants.CancellationHasBeenRequested);

                return new(false, CommandConstants.CommandCancelled);
            }
            catch
            {
                Logger.LogDebug(ErrorConstants.CannotReadTableMetadata);

                return new(false, CommandConstants.CannotReadSchema);
            }

            try
            {
                await using var writer = await connection.BeginBinaryImportAsync($@"COPY ""{table}"" ({joinedColumns}) FROM STDIN BINARY", cancellationToken);

                foreach (var row in rows)
                {
                    await writer.StartRowAsync(cancellationToken);
                    foreach (var column in columns)
                    {
                        object? value = row[column];

                        if (value == null)
                        {
                            await writer.WriteNullAsync(cancellationToken);
                            continue;
                        }

                        var type = schema.FirstOrDefault(s => s.ColumnName == column);

                        if (type is not null && type.NpgsqlDbType is not null)
                        {
                            await writer.WriteAsync(value, type.NpgsqlDbType.Value, cancellationToken);
                            continue;
                        }

                        if (type is not null && type.NpgsqlDbType is null && type.DataTypeName.Equals(@"public.vector"))
                        {
                            await writer.WriteAsync(value, @"vector", cancellationToken);
                            continue;
                        }

                        await writer.WriteAsync(value, cancellationToken);
                    }
                }

                await writer.CompleteAsync(cancellationToken);

                return new(true, CommandConstants.CopyCompleted);
            }
            catch (OperationCanceledException)
            {
                Logger.LogDebug(ProcessConstants.CancellationHasBeenRequested);

                return new(false, CommandConstants.TransactionCancelled);
            }
            catch (Exception e)
            {
                Logger.LogError(e, ErrorConstants.AnExceptionHasOccurred);

                return new(false, CommandConstants.ErrorDuringCopy);
            }
        }
    }
}
