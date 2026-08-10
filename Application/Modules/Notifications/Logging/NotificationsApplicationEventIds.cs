using Application.Logging;

namespace Application.Modules.Notifications.Logging
{
    internal static class NotificationsApplicationEventIds
    {
        public const int EmailQueued = ApplicationEventIdRanges.Notifications;
        public const int EmailQueueDuplicate = ApplicationEventIdRanges.Notifications + 1;
        public const int EmailQueueFailed = ApplicationEventIdRanges.Notifications + 2;
    }
}
