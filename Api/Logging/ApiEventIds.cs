namespace Api.Logging
{
    /// <summary>
    /// Event identifiers used by API source-generated logging methods.
    /// </summary>
    internal static class ApiEventIds
    {
        public const int ApiClientsNotConfigured = ApiEventIdRanges.General + 20;
        public const int ApiClientValidationFailed = ApiEventIdRanges.General + 21;
        public const int ApiClientValidationSucceeded = ApiEventIdRanges.General + 22;
        public const int ResponseCacheSkippedForMethod = ApiEventIdRanges.General + 30;
        public const int ResponseCacheSkippedForStatusCode = ApiEventIdRanges.General + 31;
        public const int ResponseCacheSkippedForCookies = ApiEventIdRanges.General + 32;
        public const int ResponseCacheSkippedForExistingHeader = ApiEventIdRanges.General + 33;
        public const int ResponseCacheApplied = ApiEventIdRanges.General + 34;
        public const int ResponseCacheDisabledForAuthenticatedRequest = ApiEventIdRanges.General + 35;
        public const int UnhandledRequestException = ApiEventIdRanges.General + 40;

        public const int StartupSetting = ApiEventIdRanges.General + 50;
        public const int RunningInDevelopment = ApiEventIdRanges.General + 51;
        public const int ConfigurationReloaded = ApiEventIdRanges.General + 60;
        public const int EmailAdminSearch = ApiEventIdRanges.General + 70;
        public const int EmailAdminDetail = ApiEventIdRanges.General + 71;
    }
}
