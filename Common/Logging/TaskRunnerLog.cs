using Microsoft.Extensions.Logging;

namespace Common.Logging
{
    /// <summary>
    /// Source-generated logging messages used by task execution helpers.
    /// </summary>
    internal static partial class TaskRunnerLog
    {
        [LoggerMessage(EventId = 2100, Level = LogLevel.Debug, Message = "Instance created: {InstanceName}.")]
        public static partial void TaskRunnerCreated(this ILogger logger, string instanceName);

        [LoggerMessage(EventId = 2101, Level = LogLevel.Debug, Message = "Executing {TaskCount} tasks.")]
        public static partial void ExecutingTasks(this ILogger logger, int taskCount);

        [LoggerMessage(EventId = 2102, Level = LogLevel.Debug, Message = "Cancellation has been requested.")]
        public static partial void TaskExecutionCancelled(this ILogger logger);

        [LoggerMessage(EventId = 2103, Level = LogLevel.Error, Message = "An exception has occurred.")]
        public static partial void TaskExecutionFailed(this ILogger logger, Exception exception);
    }
}
