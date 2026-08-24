namespace Api.Modules.Files.Policies
{
    /// <summary>
    /// Validates upload metadata against a maximum size and extension-specific media types.
    /// </summary>
    internal sealed class FileUploadPolicy
    {
        private readonly IReadOnlyDictionary<string, IReadOnlySet<string>> _allowedMediaTypesByExtension;

        public FileUploadPolicy(long maximumFileSizeBytes, IReadOnlyDictionary<string, string[]> allowedMediaTypesByExtension)
        {
            MaximumFileSizeBytes = maximumFileSizeBytes;
            _allowedMediaTypesByExtension = allowedMediaTypesByExtension.ToDictionary(
                pair => pair.Key,
                pair => (IReadOnlySet<string>)pair.Value.ToHashSet(StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the maximum accepted file length in bytes.
        /// </summary>
        public long MaximumFileSizeBytes { get; }

        /// <summary>
        /// Validates the supplied file's presence, length, extension and declared media type.
        /// </summary>
        public FileUploadValidationFailure Validate(IFormFile? file)
        {
            if (file is null || file.Length == 0)
                return FileUploadValidationFailure.FileRequired;

            if (file.Length > MaximumFileSizeBytes)
                return FileUploadValidationFailure.FileTooLarge;

            string extension = Path.GetExtension(file.FileName);
            string mediaType = NormalizeMediaType(file.ContentType);

            return !string.IsNullOrWhiteSpace(extension) &&
                !string.IsNullOrWhiteSpace(mediaType) &&
                _allowedMediaTypesByExtension.TryGetValue(extension, out var allowedMediaTypes) &&
                allowedMediaTypes.Contains(mediaType)
                    ? FileUploadValidationFailure.None
                    : FileUploadValidationFailure.FileTypeNotAllowed;
        }

        private static string NormalizeMediaType(string contentType)
        {
            int parameterSeparator = contentType.IndexOf(';', StringComparison.Ordinal);
            return (parameterSeparator >= 0 ? contentType[..parameterSeparator] : contentType).Trim();
        }
    }
}
