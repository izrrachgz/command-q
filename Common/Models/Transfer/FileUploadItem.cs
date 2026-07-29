using System.Net.Mime;

namespace Common.Models.Transfer
{
    public sealed class FileUploadItem
    {
        public required Stream Content { get; init; }

        public required string FileName { get; init; }

        public string Name { get; init; } = "file";

        public string ContentType { get; init; } = MediaTypeNames.Application.Octet;

        public Dictionary<string, string> Headers { get; init; } = [];
    }
}
