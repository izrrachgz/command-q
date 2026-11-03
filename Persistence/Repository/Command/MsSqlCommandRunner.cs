using Dapper;
using System.Data;
using Common.Constants;
using Persistence.Logging;
using Persistence.Constants;
using Common.Models.Command;
using Common.Models.Response;
using Persistence.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository.Command
{
    /// <summary>
    /// Executes commands and bulk inserts against Microsoft SQL Server.
    /// </summary>
    public sealed class MsSqlCommandRunner : ICommandRunner
    {
        /// <summary>
        /// Gets the logger used by this command runner.
        /// </summary>
        public ILogger Logger { get; set; }

        private string ConnectionString { get; }

        /// <summary>
        /// Creates a SQL Server command runner for a specific connection string.
        /// </summary>
        public MsSqlCommandRunner(string connectionString, ILogger<MsSqlCommandRunner> logger)
        {
            Logger = logger;
            ConnectionString = connectionString;

            Logger.CommandRunnerCreated(nameof(MsSqlCommandRunner));
        }

        /// <summary>
        /// Creates a SQL Server command runner from the configured database connection.
        /// </summary>
        public MsSqlCommandRunner(IConfiguration configuration, ILogger<MsSqlCommandRunner> logger) : this(configuration.GetDatabaseConnection(), logger)
        { }

        /// <summary>
        /// Opens a SQL Server connection.
        /// </summary>
        public async Task<IDbConnection> GetConnection(CancellationToken cancellationToken = default)
        {
            var connection = new SqlConnection(ConnectionString);

            await connection.OpenAsync(cancellationToken);

            return connection;
        }

        /// <summary>
        /// Executes a batch of SQL Server commands concurrently.
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
        /// Executes a SQL Server query and maps its rows to the requested type.
        /// </summary>
        public async Task<List<object>> Execute(string query, Type type, Dictionary<string, object?>? parameters, CancellationToken cancellationToken = default)
        {
            List<object> results = [];

            try
            {
                await using var connection = (SqlConnection)await GetConnection(cancellationToken);

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
        /// Inserts rows into a SQL Server table using bulk copy.
        /// </summary>
        public async Task<BasicResponse> BulkInsert(string table, string[] columns, Dictionary<string, object?>[] rows, CancellationToken cancellationToken = default)
        {
            Logger.SavingRecords(rows.Length);

            await using var connection = (SqlConnection)await GetConnection(cancellationToken);

            var data = new DataTable();

            try
            {
                await using var cmd = connection.CreateCommand();

                cmd.CommandText = $@"SELECT TOP 0 * FROM {table}";

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

                data.Load(reader);

                foreach (var row in rows)
                {
                    var r = data.NewRow();
                    foreach (var column in columns)
                    {
                        r[column] = row[column] ?? DBNull.Value;
                    }

                    data.Rows.Add(r);
                }
            }
            catch (OperationCanceledException)
            {
                Logger.CancellationRequested();
                throw;
            }
            catch
            {
                data.Dispose();

                Logger.CannotReadTableMetadata();

                return new(false, CommandConstants.CannotReadSchema);
            }

            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
            try
            {
                using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction);

                bulkCopy.DestinationTableName = table;

                await bulkCopy.WriteToServerAsync(data, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                await transaction.CommitAsync(cancellationToken);

                bulkCopy.Close();

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
            finally
            {
                data.Dispose();
            }
        }

        /// <summary>
        /// Inserts rows into a SQL Server table using bulk copy within a caller-owned transaction.
        /// </summary>
        public async Task<BasicResponse> BulkInsert(
            string table,
            string[] columns,
            Dictionary<string, object?>[] rows,
            IDbTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(transaction);
            if (transaction is not SqlTransaction sqlTransaction || sqlTransaction.Connection is null)
                throw new ArgumentException("A live SQL Server transaction is required.", nameof(transaction));

            Logger.SavingRecords(rows.Length);
            var data = new DataTable();
            try
            {
                await using (var cmd = sqlTransaction.Connection.CreateCommand())
                {
                    cmd.Transaction = sqlTransaction;
                    cmd.CommandText = $@"SELECT TOP 0 * FROM {table}";
                    await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                    data.Load(reader);
                }

                foreach (var row in rows)
                {
                    DataRow dataRow = data.NewRow();
                    foreach (string column in columns)
                        dataRow[column] = row[column] ?? DBNull.Value;
                    data.Rows.Add(dataRow);
                }

                using var bulkCopy = new SqlBulkCopy(sqlTransaction.Connection, SqlBulkCopyOptions.Default, sqlTransaction)
                {
                    DestinationTableName = table
                };
                await bulkCopy.WriteToServerAsync(data, cancellationToken);
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
            finally
            {
                data.Dispose();
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
