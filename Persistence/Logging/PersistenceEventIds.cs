namespace Persistence.Logging
{
    /// <summary>
    /// Event identifiers used by persistence source-generated logging methods.
    /// </summary>
    internal static class PersistenceEventIds
    {
        public const int RepositoryCreated = 4000;

        public const int CommandRunnerCreated = 4010;
        public const int ExecutingCommandBatch = 4011;
        public const int CommandCancelled = 4012;
        public const int CommandFailed = 4013;
        public const int ExecutingQuery = 4014;
        public const int ParameterInQuery = 4015;
        public const int SavingRecords = 4016;
        public const int CancellationRequested = 4017;
        public const int CannotReadTableMetadata = 4018;

        public const int MigrationScanStarted = 4020;
        public const int MigrationsNotSetup = 4021;
        public const int MigrationFilesNotFound = 4022;
        public const int LastMigrationIndexInvalid = 4023;
        public const int LastMigrationReadFailed = 4024;
        public const int MigrationsAlreadyCurrent = 4025;
        public const int MigrationFileMissing = 4026;
        public const int ApplyingMigration = 4027;
        public const int MigrationApplied = 4028;
        public const int MigrationApplyFailed = 4029;
        public const int MigrationScanCompleted = 4030;
    }
}
