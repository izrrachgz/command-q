namespace Application.Modules.Identity.Logging
{
    using Application.Logging;

    internal static class IdentityApplicationEventIds
    {
        public const int AccountCreationDuplicate = ApplicationEventIdRanges.Identity + 0;
        public const int AccountCreationVerificationFailed = ApplicationEventIdRanges.Identity + 1;
        public const int AccountCreated = ApplicationEventIdRanges.Identity + 2;
        public const int AccountCreationAffectedNoRecords = ApplicationEventIdRanges.Identity + 3;
        public const int AccountCreationFailed = ApplicationEventIdRanges.Identity + 4;

        public const int AccountSessionNotFound = ApplicationEventIdRanges.Identity + 100;
        public const int AccountSessionLocked = ApplicationEventIdRanges.Identity + 101;
        public const int AccountSessionUnlocked = ApplicationEventIdRanges.Identity + 102;
        public const int AccountSessionChanged = ApplicationEventIdRanges.Identity + 103;
        public const int AccountSessionAffectedNoRecords = ApplicationEventIdRanges.Identity + 104;
        public const int AccountSessionChangeFailed = ApplicationEventIdRanges.Identity + 105;

        public const int AccountActivityRecorded = ApplicationEventIdRanges.Identity + 200;
        public const int AccountActivityRecordFailed = ApplicationEventIdRanges.Identity + 201;
    }
}
