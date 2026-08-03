namespace Email.Models
{
    /// <summary>
    /// Represents content referenced from an HTML body through a content ID.
    /// </summary>
    public sealed record EmailInlineResource
    {
        public EmailInlineResource(
            string filePath,
            string contentId,
            string? fileName = null,
            string? contentType = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
            ArgumentException.ThrowIfNullOrWhiteSpace(contentId);

            FilePath = filePath;
            ContentId = contentId;
            FileName = string.IsNullOrWhiteSpace(fileName)
                ? Path.GetFileName(filePath)
                : fileName;
            ContentType = contentType;
        }

        private EmailInlineResource(
            string fileName,
            string contentId,
            ReadOnlyMemory<byte> content,
            string? contentType)
        {
            FileName = fileName;
            ContentId = contentId;
            Content = content;
            ContentType = contentType;
        }

        public string? FilePath { get; }
        public string FileName { get; }
        public string ContentId { get; }
        public string? ContentType { get; }
        internal ReadOnlyMemory<byte>? Content { get; }

        public static EmailInlineResource FromBytes(
            string fileName,
            string contentId,
            ReadOnlyMemory<byte> content,
            string? contentType = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
            ArgumentException.ThrowIfNullOrWhiteSpace(contentId);

            return new EmailInlineResource(fileName, contentId, content.ToArray(), contentType);
        }

        public static async Task<EmailInlineResource> FromStreamAsync(
            string fileName,
            string contentId,
            Stream content,
            string? contentType = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
            ArgumentException.ThrowIfNullOrWhiteSpace(contentId);
            ArgumentNullException.ThrowIfNull(content);

            if (!content.CanRead)
            {
                throw new ArgumentException("Inline resource streams must be readable.", nameof(content));
            }

            using var buffer = new MemoryStream();
            await content.CopyToAsync(buffer, cancellationToken);

            return new EmailInlineResource(fileName, contentId, buffer.ToArray(), contentType);
        }

        internal long GetLength()
        {
            return FilePath is null
                ? Content?.Length ?? 0
                : new FileInfo(FilePath).Length;
        }
    }
}
