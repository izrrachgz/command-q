namespace IdentityServer.Constants
{
    internal static partial class IdentityServerConfigKeys
    {
        public static class Identity
        {
            public const string Section = @"Identity";
            public const string CookieHours = @"Identity:CookieHours";
            public const string CookieName = @"Identity:CookieName";
            public const string LockoutMinutes = @"Identity:LockoutMinutes";
            public const string MaxFailedAccessAttempts = @"Identity:MaxFailedAccessAttempts";
            public const string RequiredPasswordLength = @"Identity:RequiredPasswordLength";
        }
    }
}
