namespace Application.Modules.Health.Configuration
{
    public sealed class HealthCheckConfigurationOptions
    {
        public string UrlCheckInternetAccess { get; init; } = string.Empty;
        public bool PublishOnStartup { get; init; }

        /// <summary>
        /// Gets the number of seconds between published health reports.
        /// </summary>
        public int PublishIntervalSeconds { get; init; } = 86400;
    }
}
