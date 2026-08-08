namespace Email.Outbox.Persistence.Migrations.Models
{
    internal sealed class EmailOutboxMigrationRecord
    {
        public long MigrationId { get; set; }
        public DateTime? DeletedUtc { get; set; }
    }
}
