using Dapper;
using System.Text;
using System.Data;
using Common.Constants;
using Domain.Interfaces;
using Persistence.Constant;
using Common.Models.Command;
using Common.Models.Response;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Logging;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository.Command
{
    public class MySqlCommandRunner : ICommandRunner
    {
        public ILogger Logger { get; set; }

        private const char CharacterSeparator = '\x1F';

        private const int InsertBatchSize = 1000;

        private string ConnectionString { get; }

        public MySqlCommandRunner(string connectionString, ILogger<MySqlCommandRunner> logger)
        {
            Logger = logger;
            ConnectionString = connectionString;

            Logger.LogDebug(InstanceConstants.InstanceCreated, nameof(MySqlCommandRunner));
        }

        public MySqlCommandRunner(IConfiguration configuration, ILogger<MySqlCommandRunner> logger) : this(configuration.GetDatabaseConnection(), logger)
        { }

        public async Task<IDbConnection> GetConnection(CancellationToken cancellationToken = default)
        {
            var connection = new MySqlConnection(ConnectionString);

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
                await using var connection = (MySqlConnection)await GetConnection(cancellationToken);

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

            string insertSql = $@"INSERT INTO {table} ({string.Join(',', columns)}) VALUES ";

            await using var connection = (MySqlConnection)await GetConnection(cancellationToken);

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                int insertedRows = 0;

                while (insertedRows < rows.Length)
                {
                    await using var cmd = transaction.Connection.CreateCommand();

                    var batch = rows.Skip(insertedRows).Take(InsertBatchSize);

                    var sb = new StringBuilder(insertSql);

                    int i = 0;

                    foreach (var row in batch)
                    {
                        sb.Append('(');

                        foreach (var column in columns)
                        {
                            string paramName = $@"{column}{i}";
                            sb.Append('@').Append(paramName);
                            cmd.Parameters.AddWithValue(paramName, row[column] ?? DBNull.Value);
                        }

                        sb.Append(')').Append(',');

                        i++;
                    }

                    sb.Length -= 1;

                    cmd.CommandText = sb.ToString();

                    insertedRows += await cmd.ExecuteNonQueryAsync(cancellationToken);

                    sb.Clear();
                }

                await transaction.CommitAsync(cancellationToken);

                return new(insertedRows == rows.Length, CommandConstants.CopyCompleted);
            }
            catch (OperationCanceledException)
            {
                await transaction.RollbackAsync(CancellationToken.None);

                Logger.LogDebug(ProcessConstants.CancellationHasBeenRequested);

                return new(true, CommandConstants.TransactionCancelled);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync(CancellationToken.None);

                Logger.LogError(e, ErrorConstants.AnExceptionHasOcurred);

                return new(false, CommandConstants.ErrorDuringCopy);
            }
        }

        public async Task<BasicResponse> BulkInsertLocalFile(string table, string[] columns, Dictionary<string, object?>[] rows, CancellationToken cancellationToken = default)
        {
            Logger.LogDebug(DatabaseConstants.SavingRecords, rows.Length);

            string tempPath = Path.GetTempFileName();

            var sb = new StringBuilder();

            foreach (var row in rows)
            {
                foreach (var column in columns)
                {
                    sb.Append(row[column]).Append(CharacterSeparator);
                }
                sb.AppendLine();
            }

            await File.WriteAllTextAsync(tempPath, sb.ToString(), Encoding.UTF8, cancellationToken);

            sb.Clear();

            await using var connection = (MySqlConnection)await GetConnection(cancellationToken);

            var bulkLoader = new MySqlBulkLoader(connection)
            {
                TableName = table,
                FileName = tempPath,
                FieldTerminator = CharacterSeparator.ToString(),
                LineTerminator = Environment.NewLine,
                Local = true,
                Priority = MySqlBulkLoaderPriority.Concurrent,
            };

            try
            {
                using var cmd = new MySqlCommand("SET GLOBAL local_infile = 1", connection);

                await cmd.ExecuteNonQueryAsync(cancellationToken);

                using var file = File.OpenRead(bulkLoader.FileName);

                //TODO : fix date copy
                int insertedRows = await bulkLoader.LoadAsync(file, cancellationToken);

                return new(insertedRows == rows.Length, CommandConstants.CopyCompleted);
            }
            catch (OperationCanceledException)
            {
                Logger.LogDebug(ProcessConstants.CancellationHasBeenRequested);

                return new(false, CommandConstants.CommandCancelled);
            }
            catch (Exception e)
            {
                Logger.LogError(e, ErrorConstants.AnExceptionHasOcurred);

                return new(false, CommandConstants.ErrorDuringCopy);
            }
        }
    }
}
