using Npgsql;
using Common.Extensions;
using Application.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Migrations.Extensions
{
    public static class MigrationExtensions
    {
        private static readonly string FileNameSequence = @"migration-";

        private static readonly string MigrationFileExtension = @".sql";

        private static readonly string MigrationCannotContinueMessage = @"cannot continue migration process.";

        /// <summary>
        /// Excplicitly add or call this method after repository creation
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static async Task RunMigrations(this IServiceCollection services)
        {
            string migrationsPath = $@"{Directory.GetParent(Directory.GetCurrentDirectory())}\Persistence\Migrations\";

            if (!Directory.Exists(migrationsPath))
            {
                Console.WriteLine($"Migrations are not setup, {MigrationCannotContinueMessage}");
                return;
            }

            string[] filePaths = [.. Directory.GetFiles(migrationsPath).Order()];

            if (filePaths == null || filePaths.Length == 0)
            {
                Console.WriteLine($"Migrations files are not found, {MigrationCannotContinueMessage}");
                return;
            }

            string lastFilePath = filePaths[^1];

            if (string.IsNullOrEmpty(lastFilePath))
            {
                Console.WriteLine($"Last migration file cannot be found, {MigrationCannotContinueMessage}");
                return;
            }

            FileInfo lastFile = new(lastFilePath);

            if (lastFile is null || !lastFile.Exists)
            {
                Console.WriteLine($"Last migration file cannot be found, {MigrationCannotContinueMessage}");
                return;
            }

            string migrationCountIndex = lastFile.Name
                .Remove(FileNameSequence)
                .Remove(MigrationFileExtension);

            if (!int.TryParse(migrationCountIndex, out int migrationCount))
            {
                Console.WriteLine($"Last migration index is not a valid value, {MigrationCannotContinueMessage}");
                return;
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(ApplicationConfiguration.ConfigurationFileName, optional: false)
                .AddUserSecrets(ApplicationConfiguration.UserSecrets)
                .AddEnvironmentVariables()
                .Build();

            string connectionString = configuration.GetConnectionString(ApplicationConfiguration.ConnectionStringKeyName) ?? string.Empty;

            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine($"Connection string cannot be empty in order to apply migrations.");
                return;
            }

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

            var dataSource = dataSourceBuilder.Build();

            var connection = dataSource.OpenConnection();

            long lastMigrationApplied = 0;

            using var lastAppliedMigrationCmd = connection.CreateCommand();

            lastAppliedMigrationCmd.CommandText = $@"SELECT ""Id"" FROM public.""Migrations"" ORDER BY ""Id"" DESC LIMIT 1";

            try
            {
                lastMigrationApplied = ((long?)await lastAppliedMigrationCmd.ExecuteScalarAsync()) ?? 0;
            }
            catch { }

            if (lastMigrationApplied == migrationCount)
                return;

            lastMigrationApplied++;

            for (long x = lastMigrationApplied; x <= migrationCount; x++)
            {
                string filePath = $@"{migrationsPath}{FileNameSequence}{x}{MigrationFileExtension}";

                if (!File.Exists(filePath))
                    continue;

                string content = await File.ReadAllTextAsync(filePath);

                using var cmd = connection.CreateCommand();

                cmd.CommandText = content;

                try
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine($@"An error ocurred while trying to apply migration {filePath}, {e.Message}");
                }
            }
        }
    }
}
