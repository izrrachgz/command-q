using Dapper;
using System.Data;
using Common.Constants;
using System.Data.SQLite;
using Persistence.Logging;
using Persistence.Constants;
using Common.Models.Command;
using Common.Models.Response;
using Persistence.Abstractions;
using Microsoft.Extensions.Logging;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository.Command
{
    /// <summary>
    /// Executes commands and bulk inserts against SQLite.
    /// </summary>
    public sealed class SqLiteCommandRunner : ICommandRunner
    {
        /// <summary>
        /// Gets the logger used by this command runner.
        /// </summary>
        public ILogger Logger { get; set; }

        private string ConnectionString { get; }

        /// <summary>
        /// Creates a SQLite command runner for a specific connection string.
        /// </summary>
        public SqLiteCommandRunner(string connectionString, ILogger<SqLiteCommandRunner> logger)
        {
            Logger = logger;
            ConnectionString = connectionString;

            Logger.CommandRunnerCreated(nameof(SqLiteCommandRunner));
        }

        /// <summary>
        /// Creates a SQLite command runner from the configured database connection.
        /// </summary>
        public SqLiteCommandRunner(IConfiguration configuration, ILogger<SqLiteCommandRunner> logger) : this(configuration.GetDatabaseConnection(), logger)
        { }

        /// <summary>
        /// Opens a SQLite connection.
        /// </summary>
        public async Task<IDbConnection> GetConnection(CancellationToken cancellationToken = default)
        {
            var connection = new SQLiteConnection(ConnectionString);

            await connection.OpenAsync(cancellationToken);

            return connection;
        }

        /// <summary>
        /// Executes a batch of SQLite commands concurrently.
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
        /// Executes a SQLite query and maps its rows to the requested type.
        /// </summary>
        public async Task<List<object>> Execute(string query, Type type, Dictionary<string, object?>? parameters, CancellationToken cancellationToken = default)
        {
            List<object> results = [];

            try
            {
                await using var connection = (SQLiteConnection)await GetConnection(cancellationToken);

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
        /// Inserts rows into a SQLite table in a transaction.
        /// </summary>
        public async Task<BasicResponse> BulkInsert(string table, string[] columns, Dictionary<string, object?>[] rows, CancellationToken cancellationToken = default)
        {
            Logger.SavingRecords(rows.Length);

            await using var connection = (SQLiteConnection)await GetConnection(cancellationToken);

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            try
            {
                await using var cmd = connection.CreateCommand();
                cmd.Transaction = (SQLiteTransaction)transaction;

                cmd.CommandText = $@"INSERT INTO {table} ({string.Join(@",", columns)}) VALUES (@{string.Join(@",@", columns)})";

                foreach (var column in columns)
                    cmd.Parameters.Add(new SQLiteParameter($@"@{column}"));

                await cmd.PrepareAsync(cancellationToken);

                foreach (var row in rows)
                {
                    foreach (var column in columns)
                        cmd.Parameters[$@"@{column}"].Value = row[column] ?? DBNull.Value;

                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);

                return new(true, CommandConstants.CopyCompleted);
            }
            catch (OperationCanceledException exception)
            {
                await TransactionCleanup.RollbackPreservingOriginal(transaction, exception);

                Logger.CancellationRequested();
                throw;
            }
            catch (Exception e)
            {
                await TransactionCleanup.RollbackPreservingOriginal(transaction, e);

                Logger.CommandFailed(e);

                return new(false, CommandConstants.ErrorDuringCopy);
            }
        }

        /// <summary>
        /// Inserts rows into a SQLite table within a caller-owned transaction.
        /// </summary>
        public async Task<BasicResponse> BulkInsert(
            string table,
            string[] columns,
            Dictionary<string, object?>[] rows,
            IDbTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(transaction);
            if (transaction is not SQLiteTransaction sqliteTransaction || sqliteTransaction.Connection is null)
                throw new ArgumentException("A live SQLite transaction is required.", nameof(transaction));

            Logger.SavingRecords(rows.Length);
            try
            {
                await using var cmd = sqliteTransaction.Connection.CreateCommand();
                cmd.Transaction = sqliteTransaction;
                cmd.CommandText = $@"INSERT INTO {table} ({string.Join(@",", columns)}) VALUES (@{string.Join(@",@", columns)})";
                foreach (string column in columns)
                    cmd.Parameters.Add(new SQLiteParameter($@"@{column}"));
                await cmd.PrepareAsync(cancellationToken);

                foreach (Dictionary<string, object?> row in rows)
                {
                    foreach (string column in columns)
                        cmd.Parameters[$@"@{column}"].Value = row[column] ?? DBNull.Value;
                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                }

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
