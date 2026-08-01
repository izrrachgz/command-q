namespace Application.Modules.Files.Dtos
{
    /// <summary>
    /// File metadata returned by account file queries.
    /// </summary>
    public sealed record AccountFileModel
    {
        public long Id { get; init; }

        public string FileName { get; init; } = string.Empty;

        public string? ContentType { get; init; }

        public long Size { get; init; }

        public DateTime Created { get; init; }

        public DateTime Modified { get; init; }
    }
}
