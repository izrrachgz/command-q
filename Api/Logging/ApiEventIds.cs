namespace Api.Logging
{
    /// <summary>
    /// Event identifiers used by API source-generated logging methods.
    /// </summary>
    internal static class ApiEventIds
    {
        public const int LoginFailedForUsername = 1000;
        public const int LoginBlockedForLockedAccount = 1001;
        public const int AccountLocked = 1002;
        public const int LoginFailedForAccount = 1003;
        public const int LoginSucceeded = 1004;
        public const int RefreshTokenValidationFailed = 1005;
        public const int RefreshCacheHit = 1006;
        public const int RefreshCacheMiss = 1007;
        public const int RefreshAccountSessionNotFound = 1008;
        public const int RefreshSucceeded = 1009;
        public const int LogoutRevokedRefreshToken = 1010;
        public const int LogoutInvalidRefreshToken = 1011;

        public const int RefreshTokenCreated = 1100;
        public const int RefreshTokenStoreMiss = 1101;
        public const int RefreshTokenRejected = 1102;
        public const int RefreshTokenRotationSkipped = 1103;
        public const int RefreshTokenRotated = 1104;
        public const int RefreshTokenRevoked = 1105;

        public const int AccountSessionCacheCleanupStarted = 1200;
        public const int AccountSessionCacheCleanupCompleted = 1201;
        public const int AccountSessionCacheCleanupStopped = 1202;

        public const int SignInUnauthorized = 1400;
        public const int RefreshCookieMissing = 1401;
        public const int RefreshUnauthorized = 1402;
        public const int LogoutCookieMissing = 1403;
        public const int CheckingApplicationHealth = 1410;
        public const int ApiClientsNotConfigured = 1420;
        public const int ApiClientValidationFailed = 1421;
        public const int ApiClientValidationSucceeded = 1422;
        public const int ResponseCacheSkippedForMethod = 1430;
        public const int ResponseCacheSkippedForStatusCode = 1431;
        public const int ResponseCacheSkippedForCookies = 1432;
        public const int ResponseCacheSkippedForExistingHeader = 1433;
        public const int ResponseCacheApplied = 1434;
        public const int UnhandledRequestException = 1440;

        public const int StartupSetting = 1450;
        public const int RunningInDevelopment = 1451;
    }
}
