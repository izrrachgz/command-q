using System.Net.Mime;

namespace Common.Models.Transfer
{
    /// <summary>
    /// Describes one file part to include in a multipart HTTP upload.
    /// </summary>
    public sealed class FileUploadItem
    {
        /// <summary>
        /// Stream containing the file bytes. The caller owns the stream lifetime.
        /// </summary>
        public required Stream Content { get; init; }

        /// <summary>
        /// File name sent in the multipart content disposition header.
        /// </summary>
        public required string FileName { get; init; }

        /// <summary>
        /// Form field name for this file part.
        /// </summary>
        public string Name { get; init; } = "file";

        /// <summary>
        /// MIME type assigned to this file part.
        /// </summary>
        public string ContentType { get; init; } = MediaTypeNames.Application.Octet;

        /// <summary>
        /// Extra headers applied only to this file part.
        /// </summary>
        public Dictionary<string, string> Headers { get; init; } = [];
    }
}
