using IdentityServer.Logging;

namespace IdentityServer.Modules.Accounts.Logging
{
    /// <summary>Assigns stable event identifiers from the IdentityServer Accounts logging range.</summary>
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
        public const int EmailQueueFailed = IdentityServerEventIdRanges.Accounts + 9;
        public const int EmailRequestFailed = IdentityServerEventIdRanges.Accounts + 10;
        public const int EmailDispatchQueued = IdentityServerEventIdRanges.Accounts + 11;
        public const int EmailDispatchRetry = IdentityServerEventIdRanges.Accounts + 12;
        public const int EmailDispatchInvalid = IdentityServerEventIdRanges.Accounts + 13;
        public const int EmailMaintenanceFailed = IdentityServerEventIdRanges.Accounts + 14;
        public const int AccountDataCleanedUp = IdentityServerEventIdRanges.Accounts + 15;
    }
}
