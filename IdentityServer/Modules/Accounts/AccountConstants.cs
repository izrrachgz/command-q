using System.Net.Mime;

namespace IdentityServer.Modules.Accounts
{
    /// <summary>Defines stable account roles, limits, challenge values, route segments, media types, and security action names.</summary>
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
        public const int MaxCultureLength = 16;
        public const int ChallengeTokenBytes = 32;
        public const int ChallengeConcurrencyRetries = 5;
        public const int VerificationCodeLength = 6;
        public const int VerificationCodeUpperBound = 1_000_000;
        public const string VerificationCodeFormat = "D6";
        public const string LinkTokenFragmentParameter = "token";

        /// <summary>Defines persisted purposes that isolate one-time challenge workflows.</summary>
        public static class ChallengePurposes
        {
            public const string EmailConfirmation = "email-confirmation";
            public const string PasswordRecovery = "password-recovery";
        }

        /// <summary>Defines the account-email template cultures supported by IdentityServer.</summary>
        public static class Cultures
        {
            public const string English = "en-US";
            public const string Spanish = "es-ES";
        }

        /// <summary>Defines frontend-relative routes embedded in account emails and hosted pages.</summary>
        public static class FrontendRoutes
        {
            public const string ConfirmEmail = "auth/confirm-email";
            public const string ForgotPassword = "auth/forgot-password";
            public const string PasswordRecovery = "auth/recover-password";
        }

        /// <summary>Defines media types returned or accepted by account endpoints.</summary>
        public static class ContentTypes
        {
            public const string FormUrlEncoded = MediaTypeNames.Application.FormUrlEncoded;
            public const string Html = "text/html; charset=utf-8";
            public const string Json = MediaTypeNames.Application.Json;
        }

        /// <summary>Defines route segments owned by the Accounts module.</summary>
        public static class Routes
        {
            public const string Root = "account";
            public const string InternalAdminRoot = "internal/account-admin";
            public const string InternalAdminRoles = "roles";
            public const string InternalAdminUsers = "users";
            public const string InternalAdminUser = InternalAdminUsers + "/{userId:guid}";
            public const string InternalAdminUserRoles = InternalAdminUser + "/roles";
            public const string InternalAdminUserUnlock = InternalAdminUser + "/unlock";
            public const string Register = "register";
            public const string EmailConfirmation = "email-confirmation";
            public const string EmailConfirmationVerify = EmailConfirmation + "/verify";
            public const string PasswordRecovery = "password-recovery";
            public const string PasswordRecoveryVerify = PasswordRecovery + "/verify";
            public const string PasswordRecoveryReset = PasswordRecovery + "/reset";
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

        /// <summary>Defines stable action names persisted in identity security events.</summary>
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
            public const string EmailConfirmationRequested = nameof(EmailConfirmationRequested);
            public const string EmailConfirmed = nameof(EmailConfirmed);
            public const string PasswordRecoveryRequested = nameof(PasswordRecoveryRequested);
            public const string PasswordRecoveryVerified = nameof(PasswordRecoveryVerified);
            public const string PasswordReset = nameof(PasswordReset);
            public const string AccountLockedByRecovery = nameof(AccountLockedByRecovery);
            public const string AccountUnlockedByAdmin = nameof(AccountUnlockedByAdmin);
        }
    }
}
