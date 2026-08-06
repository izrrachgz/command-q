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

    }
}
