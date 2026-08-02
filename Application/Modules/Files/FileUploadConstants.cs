namespace Application.Modules.Files
{
    internal static class FileUploadConstants
    {
        public const int BufferSizeBytes = 64 * 1024;
        public const string StorageRootFolderName = "UploadedFiles";
        public const string TempFileExtension = ".upload";
        public const string DatePathSegmentFormat = "D2";
    }
}
