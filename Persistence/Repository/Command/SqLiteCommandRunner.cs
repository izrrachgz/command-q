using Dapper;
using System.Data;
using Common.Constants;
using Domain.Interfaces;
using System.Data.SQLite;
using Persistence.Constant;
using Common.Models.Command;
using Common.Models.Response;
using Microsoft.Extensions.Logging;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository.Command
{
    public class SqLiteCommandRunner : ICommandRunner
    {
        public ILogger Logger { get; set; }

        private string ConnectionString { get; }

        public SqLiteCommandRunner(string connectionString, ILogger<SqLiteCommandRunner> logger)
        {
            Logger = logger;
            ConnectionString = connectionString;

            Logger.LogDebug(InstanceConstants.InstanceCreated, nameof(SqLiteCommandRunner));
        }

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
                Console.WriteLine($@"{ErrorConstants.AnExceptionHasOcurred}: {e.Message}");

                Logger.LogError(e, ErrorConstants.AnExceptionHasOcurred);
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

                Logger.LogDebug(DatabaseConstants.ExecutingQuery, query);

                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        object value = parameter.Value is null ? DBNull.Value : parameter.Value;
                        cmd.Parameters.AddWithValue(parameter.Key, value);

                        Logger.LogDebug(DatabaseConstants.ParameterInQuery, parameter.Key, parameter.Value);
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
                Console.WriteLine($@"{ErrorConstants.AnExceptionHasOcurred}: {e.Message}");

                Logger.LogError(e, ErrorConstants.AnExceptionHasOcurred);
            }

            return results;
        }

        public async Task<BasicResponse> BulkInsert(string table, string[] columns, Dictionary<string, object?>[] rows, CancellationToken cancellationToken = default)
        {
            Logger.LogDebug(DatabaseConstants.SavingRecords, rows.Length);

            await using var connection = (SQLiteConnection)await GetConnection(cancellationToken);

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            try
            {
                await using var cmd = connection.CreateCommand();

                cmd.CommandText = $@"INSERT INTO {table} ({string.Join(",", columns)}) VALUES (@{string.Join(",@", columns)})";

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

                Logger.LogDebug(ProcessConstants.CancellationHasBeenRequested);

                return new(false, CommandConstants.TransactionCancelled);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync(CancellationToken.None);

                Logger.LogError(e, ErrorConstants.AnExceptionHasOcurred);

                return new(false, CommandConstants.ErrorDuringCopy);
            }
        }
    }
}
