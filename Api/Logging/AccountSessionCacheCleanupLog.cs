using Microsoft.Extensions.Logging;

namespace Api.Logging
{
    internal static partial class AccountSessionCacheCleanupLog
    {
        [LoggerMessage(EventId = ApiEventIds.AccountSessionCacheCleanupStarted, Level = LogLevel.Debug, Message = "Account session cache cleanup started.")]
        public static partial void AccountSessionCacheCleanupStarted(this ILogger logger);

        [LoggerMessage(EventId = ApiEventIds.AccountSessionCacheCleanupCompleted, Level = LogLevel.Debug, Message = "Account session cache cleanup completed.")]
        public static partial void AccountSessionCacheCleanupCompleted(this ILogger logger);

        [LoggerMessage(EventId = ApiEventIds.AccountSessionCacheCleanupStopped, Level = LogLevel.Debug, Message = "Account session cache cleanup stopped.")]
        public static partial void AccountSessionCacheCleanupStopped(this ILogger logger);
    }
}

