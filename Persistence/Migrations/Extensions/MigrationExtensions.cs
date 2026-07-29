using SqlKata.Execution;
using Common.Extensions;
using Persistence.Interfaces;
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

            string migrationsPath = repository.MigrationsPath;

            if (!Directory.Exists(migrationsPath))
            {
                Console.WriteLine($"Migrations are not setup, {MigrationCannotContinueMessage}");
                return;
            }

            string[] filePaths = [.. Directory.GetFiles(migrationsPath).Where(f => f.Contains(FileNameSequence)).Order()];

            if (filePaths == null || filePaths.Length == 0)
            {
                Console.WriteLine($"Migrations files are not found, {MigrationCannotContinueMessage}");
                return;
            }

            string migrationCountIndex = new FileInfo(filePaths[^1]).Name
                .Remove(FileNameSequence)
                .Remove(MigrationFileExtension);

            if (!long.TryParse(migrationCountIndex, out long migrationCount))
            {
                Console.WriteLine($"Last migration index is not a valid value, {MigrationCannotContinueMessage}");
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
            catch { }

            if (lastMigrationId >= migrationCount)
                return;

            lastMigrationId = lastMigrationId <= 0 ? 1 : lastMigrationId;

            for (long x = lastMigrationId; x <= migrationCount; x++)
            {
                string filePath = $@"{migrationsPath}{FileNameSequence}{x}{MigrationFileExtension}";

                if (!File.Exists(filePath))
                    continue;

                string content = await File.ReadAllTextAsync(filePath);

                try
                {
                    using var cmd = repository.Db.Connection.CreateCommand();

                    if (repository.Db.Connection.State != System.Data.ConnectionState.Open)
                        repository.Db.Connection.Open();

                    cmd.CommandText = content;

                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Console.WriteLine($@"An error occurred while trying to apply migration {filePath}, {e.Message}");
                }
            }
        }
    }
}
