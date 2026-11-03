using Dapper;
using System.Data;
using System.Text;
using Common.Constants;
using Persistence.Logging;
using Persistence.Constants;
using Common.Models.Command;
using Common.Models.Response;
using MySql.Data.MySqlClient;
using Persistence.Abstractions;
using Microsoft.Extensions.Logging;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository.Command
{
    /// <summary>
    /// Executes commands and bulk inserts against MySQL.
    /// </summary>
    public sealed class MySqlCommandRunner : ICommandRunner
    {
        /// <summary>
        /// Gets the logger used by this command runner.
        /// </summary>
        public ILogger Logger { get; set; }

        private const int InsertBatchSize = 1000;

        private string ConnectionString { get; }

        /// <summary>
        /// Creates a MySQL command runner for a specific connection string.
        /// </summary>
        public MySqlCommandRunner(string connectionString, ILogger<MySqlCommandRunner> logger)
        {
            Logger = logger;
            ConnectionString = connectionString;

            Logger.CommandRunnerCreated(nameof(MySqlCommandRunner));
        }

        /// <summary>
        /// Creates a MySQL command runner from the configured database connection.
        /// </summary>
        public MySqlCommandRunner(IConfiguration configuration, ILogger<MySqlCommandRunner> logger) : this(configuration.GetDatabaseConnection(), logger)
        { }

        /// <summary>
        /// Opens a MySQL connection.
        /// </summary>
        public async Task<IDbConnection> GetConnection(CancellationToken cancellationToken = default)
        {
            var connection = new MySqlConnection(ConnectionString);

            await connection.OpenAsync(cancellationToken);

            return connection;
        }

        /// <summary>
        /// Executes a batch of MySQL commands concurrently.
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
        /// Executes a MySQL query and maps its rows to the requested type.
        /// </summary>
        public async Task<List<object>> Execute(string query, Type type, Dictionary<string, object?>? parameters, CancellationToken cancellationToken = default)
        {
            List<object> results = [];

            try
            {
                await using var connection = (MySqlConnection)await GetConnection(cancellationToken);

                await using var cmd = connection.CreateCommand();

                cmd.CommandText = query;

                Logger.ExecutingQuery();

                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        object value = parameter.Value is null ? DBNull.Value : parameter.Value;
                        cmd.Parameters.AddWithValue(parameter.Key, value);

                        Logger.ParameterInQuery(parameter.Value?.GetType().Name ?? nameof(DBNull));
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
        /// Inserts rows into a MySQL table using a transaction.
        /// </summary>
        public async Task<BasicResponse> BulkInsert(string table, string[] columns, Dictionary<string, object?>[] rows, CancellationToken cancellationToken = default)
        {
            Logger.SavingRecords(rows.Length);
            await using var connection = (MySqlConnection)await GetConnection(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                int insertedRows = await InsertBatches(table, columns, rows, transaction, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return new(insertedRows == rows.Length, CommandConstants.CopyCompleted);
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
        /// Inserts rows into a MySQL table within a caller-owned transaction.
        /// </summary>
        public async Task<BasicResponse> BulkInsert(
            string table,
            string[] columns,
            Dictionary<string, object?>[] rows,
            IDbTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(transaction);
            if (transaction is not MySqlTransaction mySqlTransaction || mySqlTransaction.Connection is null)
                throw new ArgumentException("A live MySQL transaction is required.", nameof(transaction));

            Logger.SavingRecords(rows.Length);
            try
            {
                int insertedRows = await InsertBatches(table, columns, rows, mySqlTransaction, cancellationToken);
                return new(insertedRows == rows.Length, CommandConstants.CopyCompleted);
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

        private static async Task<int> InsertBatches(
            string table,
            string[] columns,
            Dictionary<string, object?>[] rows,
            MySqlTransaction transaction,
            CancellationToken cancellationToken)
        {
            string insertSql = $@"INSERT INTO {table} ({string.Join(',', columns)}) VALUES ";
            int insertedRows = 0;
            int rowOffset = 0;
            while (rowOffset < rows.Length)
            {
                int batchSize = Math.Min(InsertBatchSize, rows.Length - rowOffset);
                await using var cmd = transaction.Connection!.CreateCommand();
                cmd.Transaction = transaction;
                var sql = new StringBuilder(insertSql);
                for (var rowIndex = 0; rowIndex < batchSize; rowIndex++)
                {
                    sql.Append('(');
                    foreach (string column in columns)
                    {
                        string parameterName = $@"p{rowIndex}_{cmd.Parameters.Count}";
                        sql.Append('@').Append(parameterName).Append(',');
                        cmd.Parameters.AddWithValue(parameterName, rows[rowOffset + rowIndex][column] ?? DBNull.Value);
                    }
                    sql.Length--;
                    sql.Append("),");
                }
                sql.Length--;
                cmd.CommandText = sql.ToString();
                insertedRows += await cmd.ExecuteNonQueryAsync(cancellationToken);
                rowOffset += batchSize;
            }

            return insertedRows;
        }
    }
}
