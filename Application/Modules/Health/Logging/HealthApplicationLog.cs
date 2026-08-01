using Microsoft.Extensions.Logging;

namespace Application.Modules.Health.Logging
{
    internal static partial class HealthApplicationLog
    {
        [LoggerMessage(EventId = HealthApplicationEventIds.ApplicationUnhealthy, Level = LogLevel.Critical, Message = "Application is unhealthy.")]
        public static partial void ApplicationUnhealthy(this ILogger logger);

        [LoggerMessage(EventId = HealthApplicationEventIds.ApplicationDegraded, Level = LogLevel.Warning, Message = "Application is degraded.")]
        public static partial void ApplicationDegraded(this ILogger logger);

        [LoggerMessage(EventId = HealthApplicationEventIds.ApplicationHealthy, Level = LogLevel.Information, Message = "Application is healthy.")]
        public static partial void ApplicationHealthy(this ILogger logger);

        [LoggerMessage(EventId = HealthApplicationEventIds.ApplicationRunning, Level = LogLevel.Information, Message = "Application is running.")]
        public static partial void ApplicationRunning(this ILogger logger);

        [LoggerMessage(EventId = HealthApplicationEventIds.DependencyCheckFailed, Level = LogLevel.Debug, Message = "Checking {DependencyName}: no access.")]
        public static partial void DependencyCheckFailed(this ILogger logger, string dependencyName);

        [LoggerMessage(EventId = HealthApplicationEventIds.DependencyCheckSucceeded, Level = LogLevel.Debug, Message = "Checking {DependencyName}: access granted.")]
        public static partial void DependencyCheckSucceeded(this ILogger logger, string dependencyName);
    }
}
