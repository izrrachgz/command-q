using Dapper;
using System.Text;
using System.Data;
using Domain.Interfaces;
using Common.Models.Command;
using Application.Constants;
using Common.Models.Response;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository.Command
{
    public class MySqlCommandRunner : ICommandRunner
    {
        private const char CharacterSeparator = '\x1F';

        private string ConnectionString { get; }

        public MySqlCommandRunner(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString(ApplicationConfiguration.ConnectionStringKeyName) ?? string.Empty;
        }

        public IDbConnection GetConnection()
        {
            var connection = new MySqlConnection(ConnectionString);

            connection.Open();

            return connection;
        }

        public async Task<List<Task<List<object>>>> RunInParallel(List<QueryCommand> commands)
        {
            var tasks = new List<Task<List<object>>>(commands.Count);

            foreach (var command in commands)
            {
                tasks.Add(Execute(command.Query, command.ReturnType, command.Parameters, command.CancellationToken));
            }

            await Parallel.ForEachAsync(tasks, async (t, token) => { await t.WaitAsync(token); });

            return tasks;
        }

        public async Task<List<object>> Execute(string query, Type type, Dictionary<string, object?>? parameters, CancellationToken cancellationToken = default)
        {
            List<object> results = [];

            var connection = GetConnection();

            using var cmd = (MySqlCommand)connection.CreateCommand();

            cmd.CommandText = query;

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    object value = parameter.Value is null ? DBNull.Value : parameter.Value;
                    cmd.Parameters.AddWithValue(parameter.Key, value);
                }
            }

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (reader.HasRows)
                results = [.. reader.Parse(type)];

            return results;
        }

        public async Task<BasicResponse> BulkInsert(string table, string[] columns, Dictionary<string, object?>[] rows, CancellationToken cancellationToken = default)
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

            using var connection = (MySqlConnection)GetConnection();

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

                await connection.DisposeAsync();

                await file.DisposeAsync();

                return new(insertedRows == rows.Length, "Copy completed");
            }
            catch
            {
                return new(false, "Internal exception, copy has rolled back.");
            }
        }
    }
}
