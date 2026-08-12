namespace Email.Worker.Logging
{
    internal static class EmailWorkerEventIds
    {
        public const int Started = EmailWorkerEventIdRanges.General + 0;
        public const int Disabled = EmailWorkerEventIdRanges.General + 1;
        public const int BatchCompleted = EmailWorkerEventIdRanges.General + 2;
        public const int PollFailed = EmailWorkerEventIdRanges.General + 3;
        public const int Stopped = EmailWorkerEventIdRanges.General + 4;
        public const int StartupConfiguration = EmailWorkerEventIdRanges.General + 5;
        public const int ConfigurationReloaded = EmailWorkerEventIdRanges.General + 6;
    }
}
