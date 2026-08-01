namespace Api.Modules.Identity.Logging
{
    using Api.Logging;

    internal static class IdentityApiEventIds
    {
        public const int LoginFailedForUsername = ApiEventIdRanges.Identity + 0;
        public const int LoginBlockedForLockedAccount = ApiEventIdRanges.Identity + 1;
        public const int AccountLocked = ApiEventIdRanges.Identity + 2;
        public const int LoginFailedForAccount = ApiEventIdRanges.Identity + 3;
        public const int LoginSucceeded = ApiEventIdRanges.Identity + 4;
        public const int RefreshTokenValidationFailed = ApiEventIdRanges.Identity + 5;
        public const int RefreshCacheHit = ApiEventIdRanges.Identity + 6;
        public const int RefreshCacheMiss = ApiEventIdRanges.Identity + 7;
        public const int RefreshAccountSessionNotFound = ApiEventIdRanges.Identity + 8;
        public const int RefreshSucceeded = ApiEventIdRanges.Identity + 9;
        public const int LogoutRevokedRefreshToken = ApiEventIdRanges.Identity + 10;
        public const int LogoutInvalidRefreshToken = ApiEventIdRanges.Identity + 11;

        public const int RefreshTokenCreated = ApiEventIdRanges.Identity + 100;
        public const int RefreshTokenStoreMiss = ApiEventIdRanges.Identity + 101;
        public const int RefreshTokenRejected = ApiEventIdRanges.Identity + 102;
        public const int RefreshTokenRotationSkipped = ApiEventIdRanges.Identity + 103;
        public const int RefreshTokenRotated = ApiEventIdRanges.Identity + 104;
        public const int RefreshTokenRevoked = ApiEventIdRanges.Identity + 105;

        public const int AccountSessionCacheCleanupStarted = ApiEventIdRanges.Identity + 200;
        public const int AccountSessionCacheCleanupCompleted = ApiEventIdRanges.Identity + 201;
        public const int AccountSessionCacheCleanupStopped = ApiEventIdRanges.Identity + 202;

        public const int SignInUnauthorized = ApiEventIdRanges.Identity + 210;
        public const int RefreshCookieMissing = ApiEventIdRanges.Identity + 211;
        public const int RefreshUnauthorized = ApiEventIdRanges.Identity + 212;
        public const int LogoutCookieMissing = ApiEventIdRanges.Identity + 213;
    }
}
