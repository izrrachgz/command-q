namespace Api.Modules.Files.Requests
{
    /// <summary>
    /// Carries one multipart file supplied to a file-upload endpoint.
    /// </summary>
    public sealed record UploadFileRequest
    {
        public IFormFile? File { get; init; }
    }
}
