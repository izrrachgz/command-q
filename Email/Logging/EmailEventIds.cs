namespace Email.Logging
{
    internal static class EmailEventIds
    {
        public const int BatchStarting = EmailEventIdRanges.General + 0;
        public const int SmtpConnected = EmailEventIdRanges.General + 1;
        public const int SmtpAuthenticated = EmailEventIdRanges.General + 2;
        public const int MessageSent = EmailEventIdRanges.General + 3;
        public const int BatchCompleted = EmailEventIdRanges.General + 4;
        public const int BatchCancelled = EmailEventIdRanges.General + 5;
        public const int BatchFailed = EmailEventIdRanges.General + 6;
        public const int DisconnectFailed = EmailEventIdRanges.General + 7;
        public const int ConfigurationReloaded = EmailEventIdRanges.General + 8;
    }
}
