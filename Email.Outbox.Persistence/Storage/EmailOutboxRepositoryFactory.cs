using System.Data.Common;
using Common.Enumerations;
using Persistence.Repository;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Email.Outbox.Persistence.Configuration;

namespace Email.Outbox.Persistence.Storage
{
    internal sealed class EmailOutboxRepositoryFactory
    {
        private const string DataSourceKey = "Data Source";
        private const string InMemoryDataSource = ":memory:";
        private readonly DatabaseEngine _engine;
        private readonly string _connectionString;
        private readonly ILogger<WritableRepository> _logger;

        public EmailOutboxRepositoryFactory(
            IOptions<EmailOutboxPersistenceOptions> options,
            IHostEnvironment environment,
            ILogger<WritableRepository> logger)
        {
            _engine = options.Value.Engine;
            _connectionString = ResolveConnectionString(
                options.Value,
                environment.ContentRootPath);
            _logger = logger;
        }

        public WritableRepository Create()
        {
            return new WritableRepository(_connectionString, _engine, _logger);
        }

        public DatabaseEngine Engine => _engine;

        private static string ResolveConnectionString(
            EmailOutboxPersistenceOptions options,
            string contentRootPath)
        {
            if (options.Engine != DatabaseEngine.SqLite)
            {
                return options.ConnectionString;
            }

            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = options.ConnectionString
            };

            if (!builder.TryGetValue(DataSourceKey, out object? configuredValue) ||
                configuredValue is not string configuredPath ||
                string.IsNullOrWhiteSpace(configuredPath) ||
                string.Equals(configuredPath, InMemoryDataSource, StringComparison.OrdinalIgnoreCase))
            {
                return options.ConnectionString;
            }

            string databasePath = Path.IsPathRooted(configuredPath)
                ? Path.GetFullPath(configuredPath)
                : Path.GetFullPath(configuredPath, contentRootPath);
            string? directory = Path.GetDirectoryName(databasePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            builder[DataSourceKey] = databasePath;
            return builder.ConnectionString;
        }
    }
}
