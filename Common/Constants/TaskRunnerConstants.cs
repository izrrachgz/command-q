namespace Common.Constants
{
    /// <summary>
    /// Default settings used by <see cref="Common.System.TaskRunner"/>.
    /// </summary>
    public static class TaskRunnerConstants
    {
        /// <summary>
        /// Default number of asynchronous operations allowed to run at the same time.
        /// </summary>
        public const int DefaultMaxConcurrency = 4;
    }
}
