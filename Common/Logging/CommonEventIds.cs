namespace Common.Logging
{
    /// <summary>
    /// Event identifiers used by common source-generated logging methods.
    /// </summary>
    internal static class CommonEventIds
    {
        public const int TypedCacheLookup = 2000;
        public const int TypedCacheSet = 2001;
        public const int TypedCacheRemoved = 2002;
        public const int TypedCacheCleared = 2003;

        public const int TaskRunnerCreated = 2100;
        public const int ExecutingTasks = 2101;
        public const int TaskExecutionCancelled = 2102;
        public const int TaskExecutionFailed = 2103;
    }
}
