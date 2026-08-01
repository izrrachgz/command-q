namespace Persistence.Logging
{
    /// <summary>
    /// Event identifiers used by persistence source-generated logging methods.
    /// </summary>
    internal static class PersistenceEventIds
    {
        public const int RepositoryCreated = PersistenceEventIdRanges.General + 0;

        public const int CommandRunnerCreated = PersistenceEventIdRanges.General + 100;
        public const int ExecutingCommandBatch = PersistenceEventIdRanges.General + 101;
        public const int CommandCancelled = PersistenceEventIdRanges.General + 102;
        public const int CommandFailed = PersistenceEventIdRanges.General + 103;
        public const int ExecutingQuery = PersistenceEventIdRanges.General + 104;
        public const int ParameterInQuery = PersistenceEventIdRanges.General + 105;
        public const int SavingRecords = PersistenceEventIdRanges.General + 106;
        public const int CancellationRequested = PersistenceEventIdRanges.General + 107;
        public const int CannotReadTableMetadata = PersistenceEventIdRanges.General + 108;

        public const int MigrationScanStarted = PersistenceEventIdRanges.General + 200;
        public const int MigrationsNotSetup = PersistenceEventIdRanges.General + 201;
        public const int MigrationFilesNotFound = PersistenceEventIdRanges.General + 202;
        public const int LastMigrationIndexInvalid = PersistenceEventIdRanges.General + 203;
        public const int LastMigrationReadFailed = PersistenceEventIdRanges.General + 204;
        public const int MigrationsAlreadyCurrent = PersistenceEventIdRanges.General + 205;
        public const int MigrationFileMissing = PersistenceEventIdRanges.General + 206;
        public const int ApplyingMigration = PersistenceEventIdRanges.General + 207;
        public const int MigrationApplied = PersistenceEventIdRanges.General + 208;
        public const int MigrationApplyFailed = PersistenceEventIdRanges.General + 209;
        public const int MigrationScanCompleted = PersistenceEventIdRanges.General + 210;
    }
}
