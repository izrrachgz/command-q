namespace IdentityServer.Modules.OpenIddict
{
    internal static class OpenIddictRoutes
    {
        public const string Authorization = "/connect/authorize";
        public const string EndSession = "/connect/logout";
        public const string Revocation = "/connect/revoke";
        public const string Token = "/connect/token";
        public const string UserInfo = "/connect/userinfo";
    }
}
