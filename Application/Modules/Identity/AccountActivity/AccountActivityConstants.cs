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
            public const string Files = nameof(Files);
            public const string Accounts = nameof(Accounts);
        }

        public static class Actions
        {
            public const string FileUploaded = nameof(FileUploaded);
            public const string PreferencesChanged = nameof(PreferencesChanged);
        }

        public static class Messages
        {
            public const string FileUploadedFormat = "File uploaded: {0}.";
            public const string PreferencesChanged = "Account preferences changed.";
            public const string RecordSucceeded = "Account activity recorded.";
            public const string RecordFailed = "Account activity could not be recorded.";
        }
    }
}
