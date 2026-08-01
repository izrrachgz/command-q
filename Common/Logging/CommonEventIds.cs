namespace Common.Logging
{
    /// <summary>
    /// Event identifiers used by common source-generated logging methods.
    /// </summary>
    internal static class CommonEventIds
    {
        public const int TypedCacheLookup = CommonEventIdRanges.General + 0;
        public const int TypedCacheSet = CommonEventIdRanges.General + 1;
        public const int TypedCacheRemoved = CommonEventIdRanges.General + 2;
        public const int TypedCacheCleared = CommonEventIdRanges.General + 3;

        public const int TaskRunnerCreated = CommonEventIdRanges.General + 100;
        public const int ExecutingTasks = CommonEventIdRanges.General + 101;
        public const int TaskExecutionCancelled = CommonEventIdRanges.General + 102;
        public const int TaskExecutionFailed = CommonEventIdRanges.General + 103;
    }
}
