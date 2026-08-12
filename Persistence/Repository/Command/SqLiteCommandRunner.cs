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
    public class SqLiteCommandRunner : ICommandRunner
    {
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

        public async Task<IDbConnection> GetConnection(CancellationToken cancellationToken = default)
        {
            var connection = new SQLiteConnection(ConnectionString);

            await connection.OpenAsync(cancellationToken);

            return connection;
        }

        public async Task<List<List<object>>> ExecuteCommands(List<QueryCommand> commands, CancellationToken cancellationToken = default)
        {
            var tasks = commands
                .Select(c => Execute(c.Query, c.ReturnType, c.Parameters, c.CancellationToken))
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
            }
            catch (Exception e)
            {
                Logger.CommandFailed(e);
            }

            return [];
        }

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
            }
            catch (Exception e)
            {
                Logger.CommandFailed(e);
            }

            return results;
        }

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
            catch (OperationCanceledException)
            {
                await transaction.RollbackAsync(CancellationToken.None);

                Logger.CancellationRequested();

                return new(false, CommandConstants.TransactionCancelled);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync(CancellationToken.None);

                Logger.CommandFailed(e);

                return new(false, CommandConstants.ErrorDuringCopy);
            }
        }
    }
}
