using IdentityServer.Modules.Accounts;
using IdentityServer.Modules.OpenIddict;

using System.Net.Mime;

namespace IdentityServer.Test.Constants
{
    internal static class IdentityServerTestConstants
    {
        public const string BrowserAccept = MediaTypeNames.Text.Html;
        public const string ExternalLoginProviderKey = "provider-subject";
        public const string ExternalLoginProviderName = "Google";
        public const string ExternalAppOrigin = "https://app.example.com";
        public const string ExternalAppCallbackUri = ExternalAppOrigin + "/auth/callback";
        public const string JsonAccept = AccountConstants.ContentTypes.Json;
        public const string LocalLoopbackSpaOrigin = "http://127.0.0.1:4201";
        public const string LocalRejectedSpaOrigin = "http://127.0.0.1:4202";
        public const string LocalSpaOrigin = "http://localhost:4200";
        public const string LocalSpaCallbackUri = LocalSpaOrigin + "/auth/callback";
        public const string SpaClientId = "command_q_spa";
        public const string TestCookieName = "CommandQ.Identity.Test";
        public const string TestPassword = "Correct-Horse-42!";

        public static string BrowserAuthorizationRedirectLocation =>
            $"{AccountConstants.Routes.SignInPath}?{AccountConstants.ReturnUrlParameter}={Uri.EscapeDataString(OpenIddictRoutes.Authorization)}";

        public static string LocalAuthorizationReturnUrl =>
            $"{OpenIddictRoutes.Authorization}?client_id={SpaClientId}&redirect_uri={Uri.EscapeDataString(LocalSpaCallbackUri)}";
    }
}
