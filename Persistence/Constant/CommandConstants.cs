namespace Persistence.Constant
{
    sealed class CommandConstants
    {
        public const string CopyCompleted = @"Copy completed.";

        public const string ErrorDuringCopy = @"Internal exception, copy has rolled back.";

        public const string CannotReadSchema = @"Could not read table schema.";
    }
}
