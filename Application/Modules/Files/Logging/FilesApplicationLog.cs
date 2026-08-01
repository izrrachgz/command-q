using Microsoft.Extensions.Logging;

namespace Application.Modules.Files.Logging
{
    internal static partial class FilesApplicationLog
    {
        [LoggerMessage(EventId = FilesApplicationEventIds.FileUploadEmptyName, Level = LogLevel.Warning, Message = "File upload rejected for account {AccountId} because the file name was empty.")]
        public static partial void FileUploadEmptyName(this ILogger logger, Guid accountId);

        [LoggerMessage(EventId = FilesApplicationEventIds.FileUploadTempPathCreated, Level = LogLevel.Debug, Message = "File upload temp path created for account {AccountId}. FileName: {FileName}.")]
        public static partial void FileUploadTempPathCreated(this ILogger logger, Guid accountId, string fileName);

        [LoggerMessage(EventId = FilesApplicationEventIds.FileUploadFailed, Level = LogLevel.Error, Message = "Error uploading file {FileName}")]
        public static partial void FileUploadFailed(this ILogger logger, Exception exception, string fileName);

        [LoggerMessage(EventId = FilesApplicationEventIds.FileUploadDestinationGenerated, Level = LogLevel.Debug, Message = "File upload destination path generated for account {AccountId}. RelativePath: {RelativePath}.")]
        public static partial void FileUploadDestinationGenerated(this ILogger logger, Guid accountId, string relativePath);

        [LoggerMessage(EventId = FilesApplicationEventIds.FileMetadataSaved, Level = LogLevel.Information, Message = "File metadata saved. AccountId: {AccountId}. AccountFileId: {AccountFileId}. FileName: {FileName}. Size: {Size}. ContentType: {ContentType}.")]
        public static partial void FileMetadataSaved(this ILogger logger, Guid accountId, long accountFileId, string fileName, long size, string? contentType);

        [LoggerMessage(EventId = FilesApplicationEventIds.FileMetadataCreationFailed, Level = LogLevel.Error, Message = "Error creating file {FileName}")]
        public static partial void FileMetadataCreationFailed(this ILogger logger, Exception exception, string fileName);
    }
}
