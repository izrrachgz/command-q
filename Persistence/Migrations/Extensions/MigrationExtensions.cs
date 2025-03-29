using Npgsql;
using Application;
using Application.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Migrations.Extensions
{
    public static class MigrationExtensions
    {
        private static readonly string FileNameSequence = @"migration-";

        public static void RunMigrations(this IServiceCollection services)
        {
            //TODO : go get last migration from db
            int lastMigrationApplied = 0;

            //TODO : get file counting from dir
            int migrationCount = 1;

            if (lastMigrationApplied == migrationCount)
                return;

            var applicationAssembly = typeof(ProjectLinkReference).Assembly;

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(ApplicationConfiguration.ConfigurationFileName, optional: false)
                .AddUserSecrets(ApplicationConfiguration.UserSecrets)
                .AddEnvironmentVariables()
                .Build();

            string connectionString = configuration.GetConnectionString(ApplicationConfiguration.ConnectionStringKeyName) ?? string.Empty;

            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine($"Connection string cannot be empty in order to apply migrations");
                return;
            }

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

            var dataSource = dataSourceBuilder.Build();

            var connection = dataSource.OpenConnection();

            lastMigrationApplied++;

            string migrationsPath = $@"{Directory.GetParent(Directory.GetCurrentDirectory())}\Persistence\Migrations\";

            for (int x = lastMigrationApplied; x <= migrationCount; x++)
            {
                string filePath = $@"{migrationsPath}{FileNameSequence}{x}.sql";

                if (!File.Exists(filePath))
                    continue;

                string content = File.ReadAllText(filePath);

                using var cmd = connection.CreateCommand();

                cmd.CommandText = content;

                cmd.ExecuteNonQuery();
            }
        }
    }
}
