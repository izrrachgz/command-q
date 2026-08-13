namespace Application.Modules.Health.Configuration
{
    /// <summary>
    /// Provides reloadable health-check settings for the application host.
    /// </summary>
    public sealed class HealthCheckConfigurationOptions
    {
        /// <summary>
        /// Gets the URL used by the internet-access diagnostic health check.
        /// </summary>
        public string UrlCheckInternetAccess { get; init; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether startup-tagged health checks run during application startup.
        /// </summary>
        public bool RunOnStartup { get; init; }

        /// <summary>
        /// Gets a value indicating whether unhealthy startup checks should stop application startup.
        /// </summary>
        public bool FailStartupOnUnhealthy { get; init; }

        /// <summary>
        /// Gets a value indicating whether publish-tagged health checks should be logged on an interval.
        /// </summary>
        public bool PublishReports { get; init; }

        /// <summary>
        /// Gets the legacy publishing flag kept for existing configuration compatibility.
        /// </summary>
        public bool PublishOnStartup { get; init; }

        /// <summary>
        /// Gets the number of seconds between published health reports.
        /// </summary>
        public int PublishIntervalSeconds { get; init; } = 86400;

        /// <summary>
        /// Gets a value indicating whether periodic health report publishing is enabled.
        /// </summary>
        public bool ShouldPublishReports => PublishReports || PublishOnStartup;
    }
}
