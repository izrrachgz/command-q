namespace IdentityServer.Constants
{
    internal static partial class IdentityServerConfigKeys
    {
        public static class DataProtection
        {
            public const string Section = @"DataProtection";
            public const string ApplicationName = @"DataProtection:ApplicationName";
            public const string KeysPath = @"DataProtection:KeysPath";

            public static class Certificate
            {
                public const string Path = @"DataProtection:Certificate:Path";
            }
        }
    }
}
