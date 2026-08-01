using Common.Extensions;
using SqlKata.Execution;
using Persistence.Logging;
using Persistence.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MigrationRecord = Domain.Entities.Migrations;

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

            logger.MigrationScanStarted(repository.Engine, migrationsPath);

            if (!Directory.Exists(migrationsPath))
            {
                logger.MigrationsNotSetup(MigrationCannotContinueMessage);
                return;
            }

            var migrationFiles = Directory.GetFiles(migrationsPath)
                .Select(filePath => new
                {
                    FilePath = filePath,
                    MigrationId = GetMigrationId(filePath)
                })
                .Where(migration => migration.MigrationId is not null)
                .OrderBy(migration => migration.MigrationId)
                .ToList();

            if (migrationFiles.Count == 0)
            {
                logger.MigrationFilesNotFound(MigrationCannotContinueMessage);
                return;
            }

            long migrationCount = migrationFiles[^1].MigrationId!.Value;

            long lastMigrationId = 0;

            try
            {
                var queryLastMigration = repository.Db.Query($"{nameof(MigrationRecord)}")
                .Select($"{nameof(MigrationRecord.Id)}")
                .WhereNull($@"{nameof(MigrationRecord.Deleted)}")
                .OrderByDesc($"{nameof(MigrationRecord.Id)}")
                .Limit(1);

                var records = await queryLastMigration.GetAsync<MigrationRecord>();

                lastMigrationId = records.FirstOrDefault()?.Id ?? 0;
            }
            catch (Exception ex)
            {
                logger.LastMigrationReadFailed(ex);
            }

            if (lastMigrationId >= migrationCount)
            {
                logger.MigrationsAlreadyCurrent(lastMigrationId, migrationCount);
                return;
            }

            long firstPendingMigrationId = lastMigrationId <= 0 ? 1 : lastMigrationId + 1;

            for (long x = firstPendingMigrationId; x <= migrationCount; x++)
            {
                string filePath = Path.Combine(migrationsPath, $@"{FileNameSequence}{x}{MigrationFileExtension}");

                if (!File.Exists(filePath))
                {
                    logger.MigrationFileMissing(filePath);
                    throw new FileNotFoundException($"Migration file {x} was not found.", filePath);
                }

                string content = await File.ReadAllTextAsync(filePath);

                try
                {
                    logger.ApplyingMigration(filePath);

                    using var cmd = repository.Db.Connection.CreateCommand();

                    if (repository.Db.Connection.State != System.Data.ConnectionState.Open)
                        repository.Db.Connection.Open();

                    cmd.CommandText = content;

                    cmd.ExecuteNonQuery();

                    if (x > 1)
                    {
                        await repository.Db.Query(nameof(MigrationRecord)).InsertAsync(new
                        {
                            Deleted = (DateTime?)null
                        });
                    }

                    logger.MigrationApplied(filePath);
                }
                catch (Exception e)
                {
                    logger.MigrationApplyFailed(e, filePath);
                    throw;
                }
            }

            logger.MigrationScanCompleted(repository.Engine);
        }

        private static long? GetMigrationId(string filePath)
        {
            string fileName = Path.GetFileName(filePath);

            if (!fileName.StartsWith(FileNameSequence, StringComparison.OrdinalIgnoreCase) ||
                !fileName.EndsWith(MigrationFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            string migrationId = fileName
                .Remove(FileNameSequence)
                .Remove(MigrationFileExtension);

            return long.TryParse(migrationId, out long value) ? value : null;
        }
    }
}
