namespace Email.Outbox.Constants
{
    public static class EmailOutboxConstants
    {
        public const string ConfigurationSection = "EmailOutbox";
        public const int DefaultMaxAttempts = 5;
        public const int DefaultBaseRetryDelaySeconds = 30;
        public const int DefaultMaxRetryDelaySeconds = 3600;
        public const int DefaultMaxErrorLength = 2000;
        public const long DefaultMaxPayloadBytes = 64 * 1024 * 1024;
        public const int MaxIdempotencyKeyLength = 256;
        public const int MaxReferenceLength = 128;
        public const int MaxProfileNameLength = 64;
        public const string DefaultProfileName = "default";
    }
}
