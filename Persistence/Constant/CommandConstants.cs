namespace Persistence.Constant
{
    internal static class CommandConstants
    {
        public const string CopyCompleted = @"Copy completed.";

        public const string ErrorDuringCopy = @"Internal exception, copy has rolled back.";

        public const string CannotReadSchema = @"Could not read table schema.";

        public const string CommandCancelled = @"Command has been cancelled.";

        public const string TransactionCancelled = @"Transaction has been cancelled.";
    }
}
