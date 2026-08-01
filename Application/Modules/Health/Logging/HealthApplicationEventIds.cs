namespace Application.Modules.Health.Logging
{
    using Application.Logging;

    internal static class HealthApplicationEventIds
    {
        public const int ApplicationUnhealthy = ApplicationEventIdRanges.Health + 0;
        public const int ApplicationDegraded = ApplicationEventIdRanges.Health + 1;
        public const int ApplicationHealthy = ApplicationEventIdRanges.Health + 2;
        public const int ApplicationRunning = ApplicationEventIdRanges.Health + 3;

        public const int DependencyCheckFailed = ApplicationEventIdRanges.Health + 10;
        public const int DependencyCheckSucceeded = ApplicationEventIdRanges.Health + 11;
    }
}
