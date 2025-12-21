namespace Application.Constants
{
    public record ApplicationMessages
    {
        public const string CommandCancelled = @"The command has ben cancelled.";

        public const string QueryCancelled = @"The query has ben cancelled.";

        public const string RecordNotFound = @"Requested record is not found.";

        public const string RecordFound = @"";

        public const string CannotCreateAccount = @"Cannot create account.";

        public const string CannotCreateAccountWithThoseParams = @"Cannot create account with that username/email address.";

        public const string AccountSaved = @"Account saved.";
    }
}
