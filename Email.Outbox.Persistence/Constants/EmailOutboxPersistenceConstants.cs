namespace Email.Outbox.Persistence.Constants
{
    public static class EmailOutboxPersistenceConstants
    {
        public const string ConfigurationSection = "EmailOutbox:Persistence";
        public const string DefaultConnectionString = "Data Source=App_Data/command_q_email_outbox.db;Version=3;Foreign Keys=True;Journal Mode=WAL;Busy Timeout=5000;Synchronous=Normal;";
        public const string OutboxTable = "EmailOutboxMessage";
        public const string MigrationTable = "EmailOutboxMigrationRecord";
        public const int CandidateMultiplier = 3;
    }
}
