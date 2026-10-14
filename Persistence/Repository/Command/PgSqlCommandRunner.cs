using Dapper;
using Npgsql;
using System.Data;
using Npgsql.Schema;
using Common.Constants;
using Persistence.Logging;
using Persistence.Constants;
using Common.Models.Command;
using Common.Models.Response;
using Persistence.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository.Command
{
    /// <summary>
    /// Executes commands and bulk inserts against PostgreSQL.
    /// </summary>
    public sealed class PgSqlCommandRunner : ICommandRunner
    {
        /// <summary>
        /// Gets the logger used by this command runner.
        /// </summary>
        public ILogger Logger { get; set; }

        private string ConnectionString { get; }

        /// <summary>
        /// Creates a PostgreSQL command runner for a specific connection string.
        /// </summary>
        public PgSqlCommandRunner(string connectionString, ILogger<PgSqlCommandRunner> logger)
        {
            Logger = logger;
            ConnectionString = connectionString;

            Logger.CommandRunnerCreated(nameof(PgSqlCommandRunner));
        }

        /// <summary>
        /// Creates a PostgreSQL command runner from the configured database connection.
        /// </summary>
        public PgSqlCommandRunner(IConfiguration configuration, ILogger<PgSqlCommandRunner> logger) : this(configuration.GetDatabaseConnection(), logger)
        { }

        /// <summary>
        /// Opens a PostgreSQL connection.
        /// </summary>
        public async Task<IDbConnection> GetConnection(CancellationToken cancellationToken = default)
        {
            var connection = new NpgsqlConnection(ConnectionString);

            await connection.OpenAsync(cancellationToken);

            return connection;
        }

        /// <summary>
        /// Executes a batch of PostgreSQL commands concurrently.
        /// </summary>
        public async Task<List<List<object>>> ExecuteCommands(List<QueryCommand> commands, CancellationToken cancellationToken = default)
        {
            var tasks = commands
                .Select(command => ExecuteCommand(command, cancellationToken))
                .ToList();

            Logger.ExecutingCommandBatch(commands.Count);

            try
            {
                var results = await Task.WhenAll(tasks);
                return [.. results];
            }
            catch (OperationCanceledException)
            {
                Logger.CommandCancelled();
                throw;
            }
            catch (Exception e)
            {
                Logger.CommandFailed(e);
            }

            return [];
        }

        /// <summary>
        /// Executes a PostgreSQL query and maps its rows to the requested type.
        /// </summary>
        public async Task<List<object>> Execute(string query, Type type, Dictionary<string, object?>? parameters, CancellationToken cancellationToken = default)
        {
            List<object> results = [];

            try
            {
                await using var connection = (NpgsqlConnection)await GetConnection(cancellationToken);

                await using var cmd = connection.CreateCommand();

                cmd.CommandText = query;

                Logger.ExecutingQuery(query);

                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        object value = parameter.Value is null ? DBNull.Value : parameter.Value;
                        cmd.Parameters.AddWithValue(parameter.Key, value);

                        Logger.ParameterInQuery(parameter.Key, parameter.Value?.GetType().Name ?? nameof(DBNull));
                    }
                }

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

                if (reader.HasRows)
                    results = [.. reader.Parse(type)];
            }
            catch (OperationCanceledException)
            {
                Logger.CommandCancelled();
                throw;
            }
            catch (Exception e)
            {
                Logger.CommandFailed(e);
            }

            return results;
        }

        /// <summary>
        /// Inserts rows into a PostgreSQL table using binary import.
        /// </summary>
        public async Task<BasicResponse> BulkInsert(string table, string[] columns, Dictionary<string, object?>[] rows, CancellationToken cancellationToken = default)
        {
            Logger.SavingRecords(rows.Length);

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
                Logger.CancellationRequested();
                throw;
            }
            catch
            {
                Logger.CannotReadTableMetadata();

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
                Logger.CancellationRequested();
                throw;
            }
            catch (Exception e)
            {
                Logger.CommandFailed(e);

                return new(false, CommandConstants.ErrorDuringCopy);
            }
        }

        /// <summary>
        /// Inserts rows into a PostgreSQL table using binary import within a caller-owned transaction.
        /// </summary>
        public async Task<BasicResponse> BulkInsert(
            string table,
            string[] columns,
            Dictionary<string, object?>[] rows,
            IDbTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(transaction);
            if (transaction is not NpgsqlTransaction pgTransaction || pgTransaction.Connection is null)
                throw new ArgumentException("A live PostgreSQL transaction is required.", nameof(transaction));

            Logger.SavingRecords(rows.Length);
            string joinedColumns = $@"""{string.Join(@""",""", columns)}""";
            try
            {
                ReadOnlyCollection<NpgsqlDbColumn> schema;
                await using (var cmd = pgTransaction.Connection.CreateCommand())
                {
                    cmd.Transaction = pgTransaction;
                    cmd.CommandText = $@"SELECT {joinedColumns} FROM ""{table}"" LIMIT 0";
                    await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                    schema = reader.GetColumnSchema();
                }

                await using var writer = await pgTransaction.Connection.BeginBinaryImportAsync(
                    $@"COPY ""{table}"" ({joinedColumns}) FROM STDIN BINARY",
                    cancellationToken);
                foreach (Dictionary<string, object?> row in rows)
                {
                    await writer.StartRowAsync(cancellationToken);
                    foreach (string column in columns)
                    {
                        object? value = row[column];
                        if (value is null)
                        {
                            await writer.WriteNullAsync(cancellationToken);
                            continue;
                        }

                        NpgsqlDbColumn? type = schema.FirstOrDefault(candidate => candidate.ColumnName == column);
                        if (type?.NpgsqlDbType is not null)
                        {
                            await writer.WriteAsync(value, type.NpgsqlDbType.Value, cancellationToken);
                            continue;
                        }

                        if (type is not null && type.DataTypeName.Equals(@"public.vector", StringComparison.Ordinal))
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
                Logger.CancellationRequested();
                throw;
            }
            catch (Exception e)
            {
                Logger.CommandFailed(e);
                return new(false, CommandConstants.ErrorDuringCopy);
            }
        }

        private async Task<List<object>> ExecuteCommand(QueryCommand command, CancellationToken cancellationToken)
        {
            using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                command.CancellationToken);
            return await Execute(
                command.Query,
                command.ReturnType,
                command.Parameters,
                linkedTokenSource.Token);
        }
    }
}
