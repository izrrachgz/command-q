namespace Common.Constants
{
    public record ProcessConstants
    {
        public const string ProcessCompleted = @"Process completed.";

        public const string ProcessCancelled = @"Process has been cancelled.";

        public const string ExecutingTasks = @"Executing {Count} tasks.";

        public const string ExecutingCommands = @"Executing {Count} commands.";

        public const string CancellationHasBeenRequested = @"Cancellation has been requested.";
    }
}
