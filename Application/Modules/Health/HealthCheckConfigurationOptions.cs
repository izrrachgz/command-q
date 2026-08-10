namespace Application.Modules.Health
{
    public sealed class HealthCheckConfigurationOptions
    {
        public string UrlCheckInternetAccess { get; init; } = string.Empty;
        public bool PublishOnStartup { get; init; }
    }
}
