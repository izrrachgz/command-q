namespace IdentityServer.Modules.Accounts
{
    public static class AccountConstants
    {
        public const string DefaultRole = "User";
        public const string AuthenticationRateLimitPolicy = "IdentityAuthentication";
        public const int SecurityActivityLimit = 100;

        public static class Routes
        {
            public const string Root = "account";
            public const string Register = "register";
            public const string SignIn = "sign-in";
            public const string SignOut = "sign-out";
            public const string Me = "me";
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
            public const string TokenRefreshed = nameof(TokenRefreshed);
        }
    }
}
