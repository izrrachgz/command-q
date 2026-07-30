using SqlKata.Execution;
using Common.Extensions;
using Persistence.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
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
        /// <param name="host"></param>
        /// <returns></returns>
        public static async Task RunMigrations(this IHost host)
        {
            using var scope = host.Services.CreateScope();

            var repository = scope.ServiceProvider.GetRequiredService<IWritableRepository>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(MigrationExtensions));

            string migrationsPath = repository.MigrationsPath;

            logger.LogInformation(@"Migration scan started. Engine: {Engine}. Path: {MigrationsPath}.", repository.Engine, migrationsPath);

            if (!Directory.Exists(migrationsPath))
            {
                logger.LogWarning(@"Migrations are not setup, {Reason}", MigrationCannotContinueMessage);
                return;
            }

            string[] filePaths = [.. Directory.GetFiles(migrationsPath).Where(f => f.Contains(FileNameSequence)).Order()];

            if (filePaths == null || filePaths.Length == 0)
            {
                logger.LogWarning(@"Migration files were not found, {Reason}", MigrationCannotContinueMessage);
                return;
            }

            string migrationCountIndex = new FileInfo(filePaths[^1]).Name
                .Remove(FileNameSequence)
                .Remove(MigrationFileExtension);

            if (!long.TryParse(migrationCountIndex, out long migrationCount))
            {
                logger.LogWarning(@"Last migration index is not valid, {Reason}", MigrationCannotContinueMessage);
                return;
            }

            long lastMigrationId = 0;

            try
            {
                var queryLastMigration = repository.Db.Query($"{nameof(Domain.Entities.Migrations)}")
                .Select($"{nameof(Domain.Entities.Migrations.Id)}")
                .WhereNull($@"{nameof(Domain.Entities.Migrations.Deleted)}")
                .OrderByDesc($"{nameof(Domain.Entities.Migrations.Id)}")
                .Limit(1);

                var records = await queryLastMigration.GetAsync<Domain.Entities.Migrations>();

                if (records is not null)
                    lastMigrationId = records.First().Id;
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, @"Unable to read last applied migration. Starting from first migration.");
            }

            if (lastMigrationId >= migrationCount)
            {
                logger.LogInformation(@"Migrations are already up to date. Last migration: {LastMigrationId}. Available migration: {MigrationCount}.", lastMigrationId, migrationCount);
                return;
            }

            lastMigrationId = lastMigrationId <= 0 ? 1 : lastMigrationId;

            for (long x = lastMigrationId; x <= migrationCount; x++)
            {
                string filePath = $@"{migrationsPath}{FileNameSequence}{x}{MigrationFileExtension}";

                if (!File.Exists(filePath))
                {
                    logger.LogWarning(@"Migration file was expected but not found. File: {MigrationFile}.", filePath);
                    continue;
                }

                string content = await File.ReadAllTextAsync(filePath);

                try
                {
                    logger.LogInformation(@"Applying migration {MigrationFile}.", filePath);

                    using var cmd = repository.Db.Connection.CreateCommand();

                    if (repository.Db.Connection.State != System.Data.ConnectionState.Open)
                        repository.Db.Connection.Open();

                    cmd.CommandText = content;

                    cmd.ExecuteNonQuery();

                    logger.LogInformation(@"Migration applied successfully. File: {MigrationFile}.", filePath);
                }
                catch (Exception e)
                {
                    logger.LogError(e, @"An error occurred while applying migration {MigrationFile}.", filePath);
                }
            }

            logger.LogInformation(@"Migration scan completed. Engine: {Engine}.", repository.Engine);
        }
    }
}
