namespace Application.Modules.Files.Logging
{
    using Application.Logging;

    internal static class FilesApplicationEventIds
    {
        public const int FileUploadEmptyName = ApplicationEventIdRanges.Files + 0;
        public const int FileUploadTempPathCreated = ApplicationEventIdRanges.Files + 1;
        public const int FileUploadFailed = ApplicationEventIdRanges.Files + 2;
        public const int FileUploadDestinationGenerated = ApplicationEventIdRanges.Files + 3;
        public const int FileMetadataSaved = ApplicationEventIdRanges.Files + 4;
        public const int FileMetadataCreationFailed = ApplicationEventIdRanges.Files + 5;
    }
}
