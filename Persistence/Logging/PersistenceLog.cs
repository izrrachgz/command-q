using Microsoft.Extensions.Logging;

namespace Persistence.Logging
{
    /// <summary>
    /// Source-generated logging messages used by persistence infrastructure.
    /// </summary>
    internal static partial class PersistenceLog
    {
        [LoggerMessage(EventId = PersistenceEventIds.RepositoryCreated, Level = LogLevel.Debug, Message = "{RepositoryName} instance created, using engine: {Engine}.")]
        public static partial void RepositoryCreated(this ILogger logger, string repositoryName, object engine);

        [LoggerMessage(EventId = PersistenceEventIds.CommandRunnerCreated, Level = LogLevel.Debug, Message = "Instance created: {InstanceName}.")]
        public static partial void CommandRunnerCreated(this ILogger logger, string instanceName);

        [LoggerMessage(EventId = PersistenceEventIds.ExecutingCommandBatch, Level = LogLevel.Debug, Message = "Executing {CommandCount} commands.")]
        public static partial void ExecutingCommandBatch(this ILogger logger, int commandCount);

        [LoggerMessage(EventId = PersistenceEventIds.CommandCancelled, Level = LogLevel.Debug, Message = "Command cancelled.")]
        public static partial void CommandCancelled(this ILogger logger);

        [LoggerMessage(EventId = PersistenceEventIds.CommandFailed, Level = LogLevel.Error, Message = "An exception has occurred.")]
        public static partial void CommandFailed(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = PersistenceEventIds.ExecutingQuery, Level = LogLevel.Debug, Message = "Executing query: {Query}.")]
        public static partial void ExecutingQuery(this ILogger logger, string query);

        [LoggerMessage(EventId = PersistenceEventIds.ParameterInQuery, Level = LogLevel.Debug, Message = "Parameter in query: {ParameterName}. Type: {ParameterType}.")]
        public static partial void ParameterInQuery(this ILogger logger, string parameterName, string parameterType);

        [LoggerMessage(EventId = PersistenceEventIds.SavingRecords, Level = LogLevel.Debug, Message = "Saving {RecordCount} records.")]
        public static partial void SavingRecords(this ILogger logger, int recordCount);

        [LoggerMessage(EventId = PersistenceEventIds.CancellationRequested, Level = LogLevel.Debug, Message = "Cancellation has been requested.")]
        public static partial void CancellationRequested(this ILogger logger);

        [LoggerMessage(EventId = PersistenceEventIds.CannotReadTableMetadata, Level = LogLevel.Debug, Message = "Cannot read table metadata.")]
        public static partial void CannotReadTableMetadata(this ILogger logger);

        [LoggerMessage(EventId = PersistenceEventIds.MigrationScanStarted, Level = LogLevel.Information, Message = "Migration scan started. Engine: {Engine}. Path: {MigrationsPath}.")]
        public static partial void MigrationScanStarted(this ILogger logger, object engine, string migrationsPath);

        [LoggerMessage(EventId = PersistenceEventIds.MigrationsNotSetup, Level = LogLevel.Warning, Message = "Migrations are not setup, {Reason}")]
        public static partial void MigrationsNotSetup(this ILogger logger, string reason);

        [LoggerMessage(EventId = PersistenceEventIds.MigrationFilesNotFound, Level = LogLevel.Warning, Message = "Migration files were not found, {Reason}")]
        public static partial void MigrationFilesNotFound(this ILogger logger, string reason);

        [LoggerMessage(EventId = PersistenceEventIds.LastMigrationIndexInvalid, Level = LogLevel.Warning, Message = "Last migration index is not valid, {Reason}")]
        public static partial void LastMigrationIndexInvalid(this ILogger logger, string reason);

        [LoggerMessage(EventId = PersistenceEventIds.LastMigrationReadFailed, Level = LogLevel.Debug, Message = "Unable to read last applied migration. Starting from first migration.")]
        public static partial void LastMigrationReadFailed(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = PersistenceEventIds.MigrationsAlreadyCurrent, Level = LogLevel.Information, Message = "Migrations are already up to date. Last migration: {LastMigrationId}. Available migration: {MigrationCount}.")]
        public static partial void MigrationsAlreadyCurrent(this ILogger logger, long lastMigrationId, long migrationCount);

        [LoggerMessage(EventId = PersistenceEventIds.MigrationFileMissing, Level = LogLevel.Warning, Message = "Migration file was expected but not found. File: {MigrationFile}.")]
        public static partial void MigrationFileMissing(this ILogger logger, string migrationFile);

        [LoggerMessage(EventId = PersistenceEventIds.ApplyingMigration, Level = LogLevel.Information, Message = "Applying migration {MigrationFile}.")]
        public static partial void ApplyingMigration(this ILogger logger, string migrationFile);

        [LoggerMessage(EventId = PersistenceEventIds.MigrationApplied, Level = LogLevel.Information, Message = "Migration applied successfully. File: {MigrationFile}.")]
        public static partial void MigrationApplied(this ILogger logger, string migrationFile);

        [LoggerMessage(EventId = PersistenceEventIds.MigrationApplyFailed, Level = LogLevel.Error, Message = "An error occurred while applying migration {MigrationFile}.")]
        public static partial void MigrationApplyFailed(this ILogger logger, Exception exception, string migrationFile);

        [LoggerMessage(EventId = PersistenceEventIds.MigrationScanCompleted, Level = LogLevel.Information, Message = "Migration scan completed. Engine: {Engine}.")]
        public static partial void MigrationScanCompleted(this ILogger logger, object engine);
    }
}

