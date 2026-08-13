namespace Application.Modules.Health
{
    /// <summary>
    /// Defines application health-check names, tags and messages.
    /// </summary>
    public static class HealthCheckConstants
    {
        public const string ConfigurationSection = "HealthCheck";
        public const string SelfName = "api";
        public const string DatabaseReadName = "api-database-read";
        public const string DatabaseWriteName = "api-database-write";
        public const string LocalStorageWriteName = "api-local-storage-write";
        public const string InternetAccessName = "api-internet-access";
        public const string LiveTag = "live";
        public const string ReadyTag = "ready";
        public const string StartupTag = "startup";
        public const string PublishTag = "publish";
        public const string ApplicationRunningMessage = "Application is running.";
    }
}
