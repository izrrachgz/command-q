namespace Email.Outbox.Persistence.Logging
{
    internal static class EmailOutboxPersistenceEventIds
    {
        public const int Enqueued = EmailOutboxPersistenceEventIdRanges.General + 0;
        public const int DuplicateFound = EmailOutboxPersistenceEventIdRanges.General + 1;
        public const int Claimed = EmailOutboxPersistenceEventIdRanges.General + 2;
        public const int MarkedSent = EmailOutboxPersistenceEventIdRanges.General + 3;
        public const int MarkedFailed = EmailOutboxPersistenceEventIdRanges.General + 4;
        public const int Released = EmailOutboxPersistenceEventIdRanges.General + 5;
        public const int MigrationStarted = EmailOutboxPersistenceEventIdRanges.General + 100;
        public const int MigrationApplied = EmailOutboxPersistenceEventIdRanges.General + 101;
        public const int MigrationsCurrent = EmailOutboxPersistenceEventIdRanges.General + 102;
        public const int MigrationFailed = EmailOutboxPersistenceEventIdRanges.General + 103;
        public const int MigrationScriptsMissing = EmailOutboxPersistenceEventIdRanges.General + 104;
        public const int MigrationHistoryAhead = EmailOutboxPersistenceEventIdRanges.General + 105;
    }
}
