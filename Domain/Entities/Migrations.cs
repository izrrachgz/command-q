namespace Domain.Entities
{
    public class Migrations : BaseEntity
    {
        public long MigrationId { get; set; }

        public string Module { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Checksum { get; set; } = string.Empty;

        public DateTime AppliedUtc { get; set; }
    }
}
