using Dapper;
using System.Text;
using System.Data;
using Domain.Interfaces;
using Persistence.Constant;
using Common.Models.Command;
using Common.Models.Response;
using MySql.Data.MySqlClient;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository.Command
{
    public class MySqlCommandRunner : ICommandRunner
    {
        private const char CharacterSeparator = '\x1F';

        private const int InsertBatchSize = 1000;

        private string ConnectionString { get; }

        public MySqlCommandRunner(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public MySqlCommandRunner(IConfiguration configuration) : this(configuration.GetDatabaseConnection())
        { }

        public async Task<IDbConnection> GetConnection(CancellationToken cancellationToken = default)
        {
            var connection = new MySqlConnection(ConnectionString);

            await connection.OpenAsync(cancellationToken);

            return connection;
        }

        public async Task<List<Task<List<object>>>> RunInParallel(List<QueryCommand> commands, CancellationToken cancellationToken = default)
        {
            var tasks = new List<Task<List<object>>>(commands.Count);

            foreach (var command in commands)
            {
                tasks.Add(Execute(command.Query, command.ReturnType, command.Parameters, command.CancellationToken));
            }

            await Parallel.ForEachAsync(tasks, cancellationToken, async (t, token) => { await t.WaitAsync(token); });

            return tasks;
        }

        public async Task<List<object>> Execute(string query, Type type, Dictionary<string, object?>? parameters, CancellationToken cancellationToken = default)
        {
            List<object> results = [];

            await using var connection = (MySqlConnection)await GetConnection(cancellationToken);

            await using var cmd = connection.CreateCommand();

            cmd.CommandText = query;

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    object value = parameter.Value is null ? DBNull.Value : parameter.Value;
                    cmd.Parameters.AddWithValue(parameter.Key, value);
                }
            }

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            if (reader.HasRows)
                results = [.. reader.Parse(type)];

            return results;
        }

        public async Task<BasicResponse> BulkInsert(string table, string[] columns, Dictionary<string, object?>[] rows, CancellationToken cancellationToken = default)
        {
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
                            cmd.Parameters.AddWithValue(paramName, row[column]);
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

                return new(true, CommandConstants.TransactionCancelled);
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);

                return new(false, CommandConstants.ErrorDuringCopy);
            }
        }

        public async Task<BasicResponse> BulkInsertLocalFile(string table, string[] columns, Dictionary<string, object?>[] rows, CancellationToken cancellationToken = default)
        {
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
            catch
            {
                return new(false, CommandConstants.ErrorDuringCopy);
            }
        }
    }
}
