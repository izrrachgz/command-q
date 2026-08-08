using Microsoft.Extensions.Logging;

namespace Email.Outbox.Persistence.Logging
{
    internal static partial class EmailOutboxPersistenceLog
    {
        [LoggerMessage(EventId = EmailOutboxPersistenceEventIds.Enqueued, Level = LogLevel.Debug, Message = "Enqueued email outbox record {OutboxId}.")]
        public static partial void Enqueued(this ILogger logger, Guid outboxId);

        [LoggerMessage(EventId = EmailOutboxPersistenceEventIds.DuplicateFound, Level = LogLevel.Debug, Message = "Resolved duplicate email outbox enqueue to record {OutboxId}.")]
        public static partial void DuplicateFound(this ILogger logger, Guid outboxId);

        [LoggerMessage(EventId = EmailOutboxPersistenceEventIds.Claimed, Level = LogLevel.Debug, Message = "Claimed {Count} email outbox records for worker {WorkerId}.")]
        public static partial void Claimed(this ILogger logger, int count, string workerId);

        [LoggerMessage(EventId = EmailOutboxPersistenceEventIds.MarkedSent, Level = LogLevel.Debug, Message = "Marked email outbox record {OutboxId} as sent.")]
        public static partial void MarkedSent(this ILogger logger, Guid outboxId);

        [LoggerMessage(EventId = EmailOutboxPersistenceEventIds.MarkedFailed, Level = LogLevel.Debug, Message = "Marked email outbox record {OutboxId} as failed. Dead letter: {IsDeadLetter}.")]
        public static partial void MarkedFailed(this ILogger logger, Guid outboxId, bool isDeadLetter);

        [LoggerMessage(EventId = EmailOutboxPersistenceEventIds.Released, Level = LogLevel.Debug, Message = "Released {Count} email outbox records.")]
        public static partial void Released(this ILogger logger, int count);

        [LoggerMessage(EventId = EmailOutboxPersistenceEventIds.MigrationStarted, Level = LogLevel.Information, Message = "Email outbox migration scan started for engine {Engine}.")]
        public static partial void MigrationStarted(this ILogger logger, object engine);

        [LoggerMessage(EventId = EmailOutboxPersistenceEventIds.MigrationApplied, Level = LogLevel.Information, Message = "Applied email outbox migration {MigrationId}: {MigrationName}.")]
        public static partial void MigrationApplied(this ILogger logger, long migrationId, string migrationName);

        [LoggerMessage(EventId = EmailOutboxPersistenceEventIds.MigrationsCurrent, Level = LogLevel.Information, Message = "Email outbox migrations are current at migration {MigrationId}.")]
        public static partial void MigrationsCurrent(this ILogger logger, long migrationId);

        [LoggerMessage(EventId = EmailOutboxPersistenceEventIds.MigrationFailed, Level = LogLevel.Error, Message = "Email outbox migration {MigrationId} failed: {MigrationName}.")]
        public static partial void MigrationFailed(this ILogger logger, Exception exception, long migrationId, string migrationName);

        [LoggerMessage(EventId = EmailOutboxPersistenceEventIds.MigrationScriptsMissing, Level = LogLevel.Error, Message = "No embedded email outbox migrations were found for engine {Engine}.")]
        public static partial void MigrationScriptsMissing(this ILogger logger, object engine);

        [LoggerMessage(EventId = EmailOutboxPersistenceEventIds.MigrationHistoryAhead, Level = LogLevel.Error, Message = "The email outbox migration ledger is ahead of the assembly. Applied: {AppliedMigrationId}. Available: {AvailableMigrationId}.")]
        public static partial void MigrationHistoryAhead(this ILogger logger, long appliedMigrationId, long availableMigrationId);
    }
}
