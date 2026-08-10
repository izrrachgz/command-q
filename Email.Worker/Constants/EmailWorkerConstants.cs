namespace Email.Worker.Constants
{
    internal static class EmailWorkerConstants
    {
        public const string ConfigurationSection = "EmailWorker";
        public const string EmailSection = "Email";
        public const string OutboxSection = "EmailOutbox";
        public const string PersistenceSection = "Persistence";
        public const string WorkerSection = "Worker";
        public const string MigrationsSection = "Migrations";
        public const string StartupLoggerCategory = "Email.Worker.Startup";
        public const int DefaultBatchSize = 10;
        public const int DefaultPollIntervalSeconds = 5;
        public const int DefaultLeaseSeconds = 1800;
        public const int DefaultErrorDelaySeconds = 30;
    }
}
