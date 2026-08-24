namespace Api.Constants
{
    internal static class SecurityHeaderValues
    {
        public const string Deny = "DENY";
        public const string HstsOneYearIncludeSubDomains = "max-age=31536000; includeSubDomains";
        public const string NoReferrer = "no-referrer";
        public const string NoSniff = "nosniff";
        public const string None = "none";
    }
}
