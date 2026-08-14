namespace Email.Constants
{
    /// <summary>
    /// Shared email configuration names and defaults.
    /// </summary>
    public static class EmailConstants
    {
        public const string ConfigurationSection = "Email";
        public const string DefaultSettingsPath = "Email/appsettings.json";
        public const string DefaultProfileName = "default";
        public const int MaxProfileNameLength = 64;
        public const int DefaultSmtpPort = 587;
        public const int DefaultTimeoutMilliseconds = 100000;
        public const int DefaultMaxRecipients = 100;
        public const int DefaultMaxAttachmentCount = 20;
        public const long DefaultMaxAttachmentBytes = 25 * 1024 * 1024;
        public const long DefaultMaxTotalAttachmentBytes = 40 * 1024 * 1024;
    }
}
