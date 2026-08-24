using NodaTime;
using SqlKata.Execution;
using Persistence.Abstractions;
using Microsoft.Extensions.Logging;
using Email.Outbox.Persistence.Logging;
using Email.Outbox.Persistence.Storage;
using Email.Outbox.Persistence.Constants;
using Email.Outbox.Persistence.Migrations.Models;

namespace Email.Outbox.Persistence.Migrations
{
    internal sealed class EmailOutboxMigrationRunner(
        EmailOutboxRepositoryFactory repositoryFactory,
        IEmailOutboxMigrationScriptSource scriptSource,
        ILogger<EmailOutboxMigrationRunner> logger,
        TimeProvider timeProvider)
    {
        private const long FirstMigrationId = 1;

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            logger.MigrationStarted(repositoryFactory.Engine);
            var scripts = await scriptSource.GetScriptsAsync(repositoryFactory.Engine, cancellationToken);

            if (scripts.Count == 0)
            {
                logger.MigrationScriptsMissing(repositoryFactory.Engine);
                throw new InvalidOperationException($"No email outbox migrations were found for engine '{repositoryFactory.Engine}'.");
            }

            ValidateSequence(scripts);

            using var repository = repositoryFactory.Create();
            long appliedMigrationId = await GetLastAppliedMigrationId(repository, cancellationToken);
            long availableMigrationId = scripts[^1].MigrationId;

            if (appliedMigrationId > availableMigrationId)
            {
                logger.MigrationHistoryAhead(appliedMigrationId, availableMigrationId);
                throw new InvalidOperationException(
                    $"The email outbox database is at migration {appliedMigrationId}, but the assembly contains migrations only through {availableMigrationId}.");
            }

            if (appliedMigrationId == availableMigrationId)
            {
                logger.MigrationsCurrent(appliedMigrationId);
                return;
            }

            foreach (var script in scripts.Where(script => script.MigrationId > appliedMigrationId))
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    using var command = repository.Db.Connection.CreateCommand();

                    if (repository.Db.Connection.State != System.Data.ConnectionState.Open)
                    {
                        repository.Db.Connection.Open();
                    }

                    command.CommandText = script.Content;
                    command.ExecuteNonQuery();

                    if (script.MigrationId > FirstMigrationId)
                    {
                        await repository.Db.Query(EmailOutboxPersistenceConstants.MigrationTable)
                            .InsertAsync(
                                new
                                {
                                    script.MigrationId,
                                    script.Name,
                                    script.Checksum,
                                    AppliedUtc = Instant.FromDateTimeOffset(timeProvider.GetUtcNow()),
                                    DeletedUtc = (Instant?)null
                                },
                                cancellationToken: cancellationToken);
                    }

                    logger.MigrationApplied(script.MigrationId, script.Name);
                }
                catch (Exception exception)
                {
                    logger.MigrationFailed(exception, script.MigrationId, script.Name);
                    throw;
                }
            }
        }

        private async Task<long> GetLastAppliedMigrationId(IWritableRepository repository, CancellationToken cancellationToken)
        {
            try
            {
                var records = await repository.Db.Query(EmailOutboxPersistenceConstants.MigrationTable)
                    .Select(nameof(EmailOutboxMigrationRecord.MigrationId))
                    .WhereNull(nameof(EmailOutboxMigrationRecord.DeletedUtc))
                    .OrderByDesc(nameof(EmailOutboxMigrationRecord.MigrationId))
                    .Limit(1)
                    .GetAsync<EmailOutboxMigrationRecord>(cancellationToken: cancellationToken);

                return records.FirstOrDefault()?.MigrationId ?? 0;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return 0;
            }
        }

        private static void ValidateSequence(IReadOnlyList<EmailOutboxMigrationScript> scripts)
        {
            for (var index = 0; index < scripts.Count; index++)
            {
                long expectedMigrationId = FirstMigrationId + index;

                if (scripts[index].MigrationId != expectedMigrationId)
                {
                    throw new InvalidOperationException($"Expected email outbox migration {expectedMigrationId} was not found or was duplicated.");
                }
            }
        }
    }
}
