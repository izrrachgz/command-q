namespace Persistence.Constants
{
    /// <summary>
    /// Messages returned by database command runners.
    /// </summary>
    internal static class CommandConstants
    {
        /// <summary>
        /// Message returned when a bulk copy operation completes.
        /// </summary>
        public const string CopyCompleted = @"Copy completed.";

        /// <summary>
        /// Message returned when an exception causes a bulk copy rollback.
        /// </summary>
        public const string ErrorDuringCopy = @"Internal exception, copy has rolled back.";

        /// <summary>
        /// Message returned when table schema metadata cannot be read.
        /// </summary>
        public const string CannotReadSchema = @"Could not read table schema.";

        /// <summary>
        /// Message returned when a command is cancelled.
        /// </summary>
        public const string CommandCancelled = @"Command has been cancelled.";

        /// <summary>
        /// Message returned when a transaction is cancelled.
        /// </summary>
        public const string TransactionCancelled = @"Transaction has been cancelled.";
    }
}
