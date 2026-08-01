using Microsoft.Extensions.Logging;

namespace Api.Modules.Health.Logging
{
    internal static partial class HealthApiLog
    {
        [LoggerMessage(EventId = HealthApiEventIds.CheckingApplicationHealth, Level = LogLevel.Debug, Message = "Checking application health status.")]
        public static partial void CheckingApplicationHealth(this ILogger logger);
    }
}
