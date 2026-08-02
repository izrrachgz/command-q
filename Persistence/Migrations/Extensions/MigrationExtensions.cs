using Common.Extensions;
using SqlKata.Execution;
using Persistence.Logging;
using System.Security.Cryptography;
using Persistence.Interfaces;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Domain.Entities;

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

            var migrationFiles = Directory.GetFiles(migrationsPath, $"*{MigrationFileExtension}", SearchOption.AllDirectories)
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

            var duplicateMigration = migrationFiles
                .GroupBy(migration => migration.MigrationId)
                .FirstOrDefault(group => group.Count() > 1);

            if (duplicateMigration is not null)
            {
                throw new InvalidOperationException($"Migration id {duplicateMigration.Key} is defined more than once.");
            }

            long migrationCount = migrationFiles[^1].MigrationId!.Value;

            long lastMigrationId = await GetLastAppliedMigrationId(repository, logger);

            if (lastMigrationId >= migrationCount)
            {
                logger.MigrationsAlreadyCurrent(lastMigrationId, migrationCount);
                return;
            }

            long firstPendingMigrationId = lastMigrationId <= 0 ? 1 : lastMigrationId + 1;

            for (long x = firstPendingMigrationId; x <= migrationCount; x++)
            {
                string? filePath = migrationFiles.FirstOrDefault(migration => migration.MigrationId == x)?.FilePath;

                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                {
                    string expectedFileName = $@"{FileNameSequence}{x}{MigrationFileExtension}";

                    logger.MigrationFileMissing(expectedFileName);
                    throw new FileNotFoundException($"Migration file {x} was not found.", expectedFileName);
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
                        await repository.Db.Query(nameof(MigrationRecord)).InsertAsync(CreateMigrationRecord(filePath, x, content));
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

        private static async Task<long> GetLastAppliedMigrationId(IWritableRepository repository, ILogger logger)
        {
            try
            {
                var queryLastMigration = repository.Db.Query($"{nameof(MigrationRecord)}")
                    .Select($"{nameof(MigrationRecord.MigrationId)}")
                    .WhereNull($@"{nameof(MigrationRecord.Deleted)}")
                    .OrderByDesc($"{nameof(MigrationRecord.MigrationId)}")
                    .Limit(1);

                var records = await queryLastMigration.GetAsync<MigrationRecord>();

                return records.FirstOrDefault()?.MigrationId ?? 0;
            }
            catch (Exception migrationIdException)
            {
                logger.LastMigrationReadFailed(migrationIdException);
                return 0;
            }
        }

        private static object CreateMigrationRecord(string filePath, long migrationId, string content)
        {
            string module = Path.GetFileName(Path.GetDirectoryName(filePath)) ?? string.Empty;
            string name = Path.GetFileNameWithoutExtension(filePath);
            string checksum = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));

            return new
            {
                MigrationId = migrationId,
                Module = module,
                Name = name,
                Checksum = checksum,
                AppliedUtc = DateTime.UtcNow,
                Deleted = (DateTime?)null
            };
        }
    }
}
