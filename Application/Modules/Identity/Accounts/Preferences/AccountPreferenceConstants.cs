namespace Application.Modules.Identity.Accounts.Preferences
{
    public static class AccountPreferenceConstants
    {
        public const int CultureMaxLength = 16;
        // The current TZDB maximum is 32 characters; retain headroom for future IANA identifiers.
        public const int TimeZoneMaxLength = 64;
    }
}
