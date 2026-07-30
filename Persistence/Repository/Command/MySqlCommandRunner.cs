using Dapper;
using System.Data;
using System.Text;
using Common.Constants;
using Persistence.Logging;
using Persistence.Constant;
using System.Globalization;
using Common.Models.Command;
using Common.Models.Response;
using MySql.Data.MySqlClient;
using Persistence.Interfaces;
using Microsoft.Extensions.Logging;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository.Command
{
    /// <summary>
    /// Executes commands and bulk inserts against MySQL.
    /// </summary>
    public class MySqlCommandRunner : ICommandRunner
    {
        public ILogger Logger { get; set; }

        private const char CharacterSeparator = '\x1F';

        private const int InsertBatchSize = 1000;

        private string ConnectionString { get; }

        private const string NullStringRepresentation = @"\N";

        private const string DateTimeDatabaseFileStringFormat = @"o";

        private const string TimeSpanDatabaseFileStringFormat = @"c";

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
                await using var connection = (MySqlConnection)await GetConnection(cancellationToken);

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

            string insertSql = $@"INSERT INTO {table} ({string.Join(',', columns)}) VALUES ";

            await using var connection = (MySqlConnection)await GetConnection(cancellationToken);

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                int insertedRows = 0;

                while (insertedRows < rows.Length)
                {
                    await using var cmd = transaction.Connection.CreateCommand();
                    cmd.Transaction = transaction;

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
                            sb.Append(',');
                        }

                        sb.Length -= 1;
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

        /// <summary>
        /// Inserts rows using MySQL local-file bulk loading.
        /// </summary>
        public async Task<BasicResponse> BulkInsertLocalFile(string table, string[] columns, Dictionary<string, object?>[] rows, CancellationToken cancellationToken = default)
        {
            Logger.SavingRecords(rows.Length);

            string tempPath = Path.GetTempFileName();

            var sb = new StringBuilder();

            foreach (var row in rows)
            {
                sb.AppendJoin(CharacterSeparator, columns.Select(column => SerializeParameter(row[column])));
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
                using var cmd = new MySqlCommand(@"SET GLOBAL local_infile = 1", connection);

                await cmd.ExecuteNonQueryAsync(cancellationToken);

                using var file = File.OpenRead(bulkLoader.FileName);

                int insertedRows = await bulkLoader.LoadAsync(file, cancellationToken);

                return new(insertedRows == rows.Length, CommandConstants.CopyCompleted);
            }
            catch (OperationCanceledException)
            {
                Logger.CancellationRequested();

                return new(false, CommandConstants.CommandCancelled);
            }
            catch (Exception e)
            {
                Logger.CommandFailed(e);

                return new(false, CommandConstants.ErrorDuringCopy);
            }
            finally
            {
                File.Delete(tempPath);
            }
        }

        private object? SerializeParameter(object? parameter)
        {
            if (parameter == null)
                return NullStringRepresentation;

            if (parameter is DateTime date)
                return date.ToString(DateTimeDatabaseFileStringFormat, CultureInfo.InvariantCulture);

            if (parameter is DateTimeOffset offset)
                return offset.ToString(DateTimeDatabaseFileStringFormat, CultureInfo.InvariantCulture);

            if (parameter is TimeSpan span)
                return span.ToString(TimeSpanDatabaseFileStringFormat, CultureInfo.InvariantCulture);

            //TODO: check for other tricky types

            return parameter;
        }
    }
}
