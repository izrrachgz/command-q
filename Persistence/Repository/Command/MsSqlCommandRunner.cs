using Dapper;
using System.Data;
using Common.Constants;
using Persistence.Constant;
using Common.Models.Command;
using Persistence.Interfaces;
using Common.Models.Response;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository.Command
{
    /// <summary>
    /// Executes commands and bulk inserts against Microsoft SQL Server.
    /// </summary>
    public class MsSqlCommandRunner : ICommandRunner
    {
        public ILogger Logger { get; set; }

        private string ConnectionString { get; }

        /// <summary>
        /// Creates a SQL Server command runner for a specific connection string.
        /// </summary>
        public MsSqlCommandRunner(string connectionString, ILogger<MsSqlCommandRunner> logger)
        {
            Logger = logger;
            ConnectionString = connectionString;

            Logger.LogDebug(InstanceConstants.InstanceCreated, nameof(MsSqlCommandRunner));
        }

        /// <summary>
        /// Creates a SQL Server command runner from the configured database connection.
        /// </summary>
        public MsSqlCommandRunner(IConfiguration configuration, ILogger<MsSqlCommandRunner> logger) : this(configuration.GetDatabaseConnection(), logger)
        { }

        public async Task<IDbConnection> GetConnection(CancellationToken cancellationToken = default)
        {
            var connection = new SqlConnection(ConnectionString);

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
                await using var connection = (SqlConnection)await GetConnection(cancellationToken);

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
                Logger.LogDebug(ProcessConstants.CancellationHasBeenRequested);

                return new(false, CommandConstants.CommandCancelled);
            }
            catch
            {
                data.Dispose();

                Logger.LogDebug(ErrorConstants.CannotReadTableMetadata);

                return new(false, CommandConstants.CannotReadSchema);
            }

            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
            try
            {
                using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction);

                bulkCopy.DestinationTableName = table;

                await bulkCopy.WriteToServerAsync(data, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return new(false, CommandConstants.TransactionCancelled);
                }

                await transaction.CommitAsync(cancellationToken);

                bulkCopy.Close();

                return new(true, CommandConstants.CopyCompleted);
            }
            catch (OperationCanceledException)
            {
                await transaction.RollbackAsync(CancellationToken.None);

                Logger.LogDebug(ProcessConstants.CancellationHasBeenRequested);

                return new(false, CommandConstants.TransactionCancelled);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync(CancellationToken.None);

                Logger.LogError(e, ErrorConstants.AnExceptionHasOccurred);

                return new(false, CommandConstants.ErrorDuringCopy);
            }
            finally
            {
                data.Dispose();
            }
        }
    }
}
