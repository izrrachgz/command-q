namespace Application.Modules.Identity.AccountActivity
{
    public static class AccountActivityConstants
    {
        public const int ModuleMaxLength = 128;
        public const int ActionMaxLength = 128;
        public const int EntityTypeMaxLength = 128;
        public const int EntityIdMaxLength = 128;
        public const int MessageMaxLength = 1024;
        public const int MetadataJsonMaxLength = 4000;
        public const int IpAddressMaxLength = 64;
        public const int UserAgentMaxLength = 512;

        public static class Modules
        {
            public const string Identity = nameof(Identity);
            public const string Files = nameof(Files);
        }

        public static class Actions
        {
            public const string LoginBlocked = nameof(LoginBlocked);
            public const string AccountLocked = nameof(AccountLocked);
            public const string LoginFailed = nameof(LoginFailed);
            public const string LoginSucceeded = nameof(LoginSucceeded);
            public const string TokenRefreshed = nameof(TokenRefreshed);
            public const string Logout = nameof(Logout);
            public const string FileUploaded = nameof(FileUploaded);
        }

        public static class Messages
        {
            public const string LoginBlocked = "Login blocked because account is locked.";
            public const string AccountLocked = "Account locked after failed login attempts.";
            public const string LoginFailed = "Failed login attempt.";
            public const string LoginSucceeded = "Account signed in.";
            public const string TokenRefreshed = "Access token refreshed.";
            public const string Logout = "Account signed out.";
            public const string FileUploadedFormat = "File uploaded: {0}.";
            public const string RecordSucceeded = "Account activity recorded.";
            public const string RecordFailed = "Account activity could not be recorded.";
        }
    }
}
