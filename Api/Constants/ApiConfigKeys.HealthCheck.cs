namespace Api.Constants
{
    internal static partial class ApiConfigKeys
    {
        public static class HealthCheck
        {
            public const string Section = @"HealthCheck";
            public const string Url = @"HealthCheck:UrlCheckInternetAccess";
            public const string RunOnStartup = @"HealthCheck:RunOnStartup";
            public const string PublishReports = @"HealthCheck:PublishReports";
            public const string FailStartupOnUnhealthy = @"HealthCheck:FailStartupOnUnhealthy";
            public const string PublishIntervalSeconds = @"HealthCheck:PublishIntervalSeconds";
        }
    }
}
