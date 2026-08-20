namespace IdentityServer.Modules.Accounts
{
    public static class AccountConstants
    {
        public const string AdminRole = "Admin";
        public const string DefaultRole = "User";
        public const string DefaultReturnUrl = "/";
        public const string AuthenticationRateLimitPolicy = "IdentityAuthentication";
        public const string ReturnUrlParameter = "returnUrl";
        public const int SecurityActivityLimit = 100;
        public const int MaxAdminPageSize = 100;
        public const int MaxTimeZoneLength = 64;

        public static class ContentTypes
        {
            public const string FormUrlEncoded = "application/x-www-form-urlencoded";
            public const string Html = "text/html; charset=utf-8";
            public const string Json = "application/json";
        }

        public static class Routes
        {
            public const string Root = "account";
            public const string InternalAdminRoot = "internal/account-admin";
            public const string InternalAdminRoles = "roles";
            public const string InternalAdminUsers = "users";
            public const string InternalAdminUser = InternalAdminUsers + "/{userId:guid}";
            public const string InternalAdminUserRoles = InternalAdminUser + "/roles";
            public const string Register = "register";
            public const string SignIn = "sign-in";
            public const string SignInPath = "/" + Root + "/" + SignIn;
            public const string SignOut = "sign-out";
            public const string Me = "me";
            public const string MePath = "/" + Root + "/" + Me;
            public const string SecurityActivity = "security-activity";
            public const string External = "external/{provider}";
            public const string ExternalCallback = "external-callback";
            public const string ExternalLink = "external/{provider}/link";
            public const string ExternalLinkCallback = "external-link-callback";
        }

        public static class Actions
        {
            public const string Registered = nameof(Registered);
            public const string LoginFailed = nameof(LoginFailed);
            public const string LoginLockedOut = nameof(LoginLockedOut);
            public const string LoginSucceeded = nameof(LoginSucceeded);
            public const string Logout = nameof(Logout);
            public const string ExternalLoginSucceeded = nameof(ExternalLoginSucceeded);
            public const string ExternalAccountCreated = nameof(ExternalAccountCreated);
            public const string ExternalAccountLinked = nameof(ExternalAccountLinked);
            public const string RolesUpdated = nameof(RolesUpdated);
            public const string TokenRefreshed = nameof(TokenRefreshed);
        }
    }
}
