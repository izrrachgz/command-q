namespace Domain.Entities
{
    /// <summary>
    /// Stores metadata for a file owned by an account.
    /// </summary>
    public class AccountFile : BaseEntity
    {
        public Guid AccountId { get; set; }

        public required string FileName { get; set; }

        public required string Path { get; set; }

        public string? ContentType { get; set; }

        public long Size { get; set; }
    }
}
