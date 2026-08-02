using IdentityServer.Logging;

namespace IdentityServer.Modules.Accounts.Logging
{
    internal static class AccountEventIds
    {
        public const int Registered = IdentityServerEventIdRanges.Accounts + 0;
        public const int RegistrationFailed = IdentityServerEventIdRanges.Accounts + 1;
        public const int LoginSucceeded = IdentityServerEventIdRanges.Accounts + 2;
        public const int LoginFailed = IdentityServerEventIdRanges.Accounts + 3;
        public const int LoginLockedOut = IdentityServerEventIdRanges.Accounts + 4;
        public const int Logout = IdentityServerEventIdRanges.Accounts + 5;
        public const int ExternalLoginSucceeded = IdentityServerEventIdRanges.Accounts + 6;
        public const int ExternalLoginFailed = IdentityServerEventIdRanges.Accounts + 7;
        public const int SecurityEventRecordFailed = IdentityServerEventIdRanges.Accounts + 8;
    }
}
