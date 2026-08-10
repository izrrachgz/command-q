namespace IdentityServer.Constants
{
    internal static class ConfigKeys
    {
        public const string Section = "IdentityServer";

        public static class ConnectionStrings
        {
            public const string Engine = "ConnectionStrings:Engine";
            public const string IdentityDb = "IdentityDb";
        }

        public static class Migrations
        {
            public const string RunOnStartup = "Migrations:RunOnStartup";
        }

        public static class OpenIddict
        {
            public const string Section = "OpenIddict";
        }

        public static class Identity
        {
            public const string Section = "Identity";
        }

        public static class ExternalProviders
        {
            public const string Section = "ExternalProviders";
        }

        public static class DataProtection
        {
            public const string Section = "DataProtection";
        }
    }
}
