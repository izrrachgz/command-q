namespace Api.Modules.Files.Policies
{
    /// <summary>
    /// Identifies why an uploaded file does not satisfy a file-upload policy.
    /// </summary>
    internal enum FileUploadValidationFailure
    {
        None,
        FileRequired,
        FileTooLarge,
        FileTypeNotAllowed
    }
}
