using Microsoft.Extensions.Logging;

namespace Application.Modules.Notifications.Logging
{
    internal static partial class NotificationsApplicationLog
    {
        [LoggerMessage(EventId = NotificationsApplicationEventIds.EmailQueued, Level = LogLevel.Information, Message = "Email queued. OutboxId: {OutboxId}.")]
        public static partial void EmailQueued(this ILogger logger, Guid outboxId);

        [LoggerMessage(EventId = NotificationsApplicationEventIds.EmailQueueDuplicate, Level = LogLevel.Information, Message = "Duplicate email queue request resolved to existing record. OutboxId: {OutboxId}.")]
        public static partial void EmailQueueDuplicate(this ILogger logger, Guid outboxId);

        [LoggerMessage(EventId = NotificationsApplicationEventIds.EmailQueueFailed, Level = LogLevel.Error, Message = "Email queue request failed.")]
        public static partial void EmailQueueFailed(this ILogger logger, Exception exception);
    }
}
