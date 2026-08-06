namespace Application.Migrations.Models
{
    /// <summary>
    /// Represents an entry in the application migration ledger.
    /// </summary>
    internal sealed class MigrationRecord
    {
        public long Id { get; set; }
        public long MigrationId { get; set; }
        public string Module { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Checksum { get; set; } = string.Empty;
        public DateTime AppliedUtc { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime? Deleted { get; set; }
    }
}
