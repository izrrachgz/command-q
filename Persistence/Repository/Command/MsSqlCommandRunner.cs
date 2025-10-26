using Dapper;
using System.Data;
using Domain.Interfaces;
using Persistence.Constant;
using Common.Models.Command;
using Common.Models.Response;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository.Command
{
    public class MsSqlCommandRunner : ICommandRunner
    {
        public ILogger Logger { get; set; }

        private string ConnectionString { get; }

        public MsSqlCommandRunner(string connectionString, ILogger<MsSqlCommandRunner> logger)
        {
            Logger = logger;
            ConnectionString = connectionString;

            logger.LogDebug("MsSqlCommandRunner instance created.");
        }

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

            try
            {
                var results = await Task.WhenAll(tasks);
                return [.. results];
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine(CommandConstants.CommandCancelled);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine(CommandConstants.CommandCancelled);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return results;
        }

        public async Task<BasicResponse> BulkInsert(string table, string[] columns, Dictionary<string, object?>[] rows, CancellationToken cancellationToken = default)
        {
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
                return new(false, CommandConstants.CommandCancelled);
            }
            catch
            {
                data.Dispose();

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

                    return new(true, CommandConstants.TransactionCancelled);
                }

                await transaction.CommitAsync(cancellationToken);

                bulkCopy.Close();

                return new(true, CommandConstants.CopyCompleted);
            }
            catch (OperationCanceledException)
            {
                await transaction.RollbackAsync(CancellationToken.None);

                return new(false, CommandConstants.TransactionCancelled);
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);

                return new(false, CommandConstants.ErrorDuringCopy);
            }
            finally
            {
                data.Dispose();
            }
        }
    }
}
