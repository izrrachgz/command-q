namespace Email.Models
{
    /// <summary>
    /// Represents a file-system or in-memory attachment ready to add to a message.
    /// </summary>
    public sealed record EmailAttachment
    {
        public EmailAttachment(string filePath, string? fileName = null, string? contentType = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            FilePath = filePath;
            FileName = string.IsNullOrWhiteSpace(fileName)
                ? Path.GetFileName(filePath)
                : fileName;
            ContentType = contentType;
        }

        private EmailAttachment(string fileName, ReadOnlyMemory<byte> content, string? contentType)
        {
            FileName = fileName;
            Content = content;
            ContentType = contentType;
        }

        public string? FilePath { get; }
        public string FileName { get; }
        public string? ContentType { get; }
        internal ReadOnlyMemory<byte>? Content { get; }

        public static EmailAttachment FromBytes(
            string fileName,
            ReadOnlyMemory<byte> content,
            string? contentType = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

            return new EmailAttachment(fileName, content.ToArray(), contentType);
        }

        public static async Task<EmailAttachment> FromStreamAsync(
            string fileName,
            Stream content,
            string? contentType = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
            ArgumentNullException.ThrowIfNull(content);

            if (!content.CanRead)
            {
                throw new ArgumentException("Attachment streams must be readable.", nameof(content));
            }

            using var buffer = new MemoryStream();
            await content.CopyToAsync(buffer, cancellationToken);

            return new EmailAttachment(fileName, buffer.ToArray(), contentType);
        }

        internal long GetLength()
        {
            return FilePath is null
                ? Content?.Length ?? 0
                : new FileInfo(FilePath).Length;
        }
    }
}
