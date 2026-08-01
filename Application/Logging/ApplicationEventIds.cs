namespace Application.Logging
{
    /// <summary>
    /// Event identifiers used by application source-generated logging methods.
    /// </summary>
    internal static class ApplicationEventIds
    {
        public const int EntityCreatedForAccount = ApplicationEventIdRanges.General + 20;
        public const int EntityCreationAffectedNoRecords = ApplicationEventIdRanges.General + 21;
        public const int EntityCreationFailed = ApplicationEventIdRanges.General + 22;
    }
}
