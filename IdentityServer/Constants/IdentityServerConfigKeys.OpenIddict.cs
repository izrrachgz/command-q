namespace IdentityServer.Constants
{
    internal static partial class IdentityServerConfigKeys
    {
        public static class OpenIddict
        {
            public const string Section = @"OpenIddict";
            public const string ApiScope = @"OpenIddict:ApiScope";
            public const string Issuer = @"OpenIddict:Issuer";

            public static class Certificates
            {
                public const string Section = @"OpenIddict:Certificates";
                public const string EncryptionPath = @"OpenIddict:Certificates:Encryption:Path";
                public const string SigningPath = @"OpenIddict:Certificates:Signing:Path";
            }
        }
    }
}
