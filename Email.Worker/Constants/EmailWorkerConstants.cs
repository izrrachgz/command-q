namespace Email.Worker.Constants
{
    internal static class EmailWorkerConstants
    {
        public const string ConfigurationSection = "EmailOutbox:Worker";
        public const int DefaultBatchSize = 10;
        public const int DefaultPollIntervalSeconds = 5;
        public const int DefaultLeaseSeconds = 1800;
        public const int DefaultErrorDelaySeconds = 30;
    }
}
