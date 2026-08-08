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

        /// <summary>
        /// Copies the inline resource content to a writable destination stream.
        /// </summary>
        public async Task CopyContentToAsync(
            Stream destination,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(destination);

            if (!destination.CanWrite)
            {
                throw new ArgumentException("The destination stream must be writable.", nameof(destination));
            }

            if (FilePath is not null)
            {
                await using var source = File.OpenRead(FilePath);
                await source.CopyToAsync(destination, cancellationToken);
                return;
            }

            if (Content.HasValue)
            {
                await destination.WriteAsync(Content.Value, cancellationToken);
            }
        }

        internal long GetLength()
        {
            return FilePath is null
                ? Content?.Length ?? 0
                : new FileInfo(FilePath).Length;
        }
    }
}
