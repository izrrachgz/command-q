namespace Application.Logging
{
    /// <summary>
    /// Event identifiers used by application source-generated logging methods.
    /// </summary>
    internal static class ApplicationEventIds
    {
        public const int ApplicationUnhealthy = 3000;
        public const int ApplicationDegraded = 3001;
        public const int ApplicationHealthy = 3002;
        public const int ApplicationRunning = 3003;

        public const int DependencyCheckFailed = 3010;
        public const int DependencyCheckSucceeded = 3011;

        public const int EntityCreatedForAccount = 3020;
        public const int EntityCreationAffectedNoRecords = 3021;
        public const int EntityCreationFailed = 3022;

        public const int AccountCreationDuplicate = 3030;
        public const int AccountCreationVerificationFailed = 3031;
        public const int AccountCreated = 3032;
        public const int AccountCreationAffectedNoRecords = 3033;
        public const int AccountCreationFailed = 3034;

        public const int AccountSessionNotFound = 3040;
        public const int AccountSessionLocked = 3041;
        public const int AccountSessionUnlocked = 3042;
        public const int AccountSessionChanged = 3043;
        public const int AccountSessionAffectedNoRecords = 3044;
        public const int AccountSessionChangeFailed = 3045;

        public const int FileUploadEmptyName = 3050;
        public const int FileUploadTempPathCreated = 3051;
        public const int FileUploadFailed = 3052;
        public const int FileUploadDestinationGenerated = 3053;
        public const int FileMetadataSaved = 3054;
        public const int FileMetadataCreationFailed = 3055;
    }
}
