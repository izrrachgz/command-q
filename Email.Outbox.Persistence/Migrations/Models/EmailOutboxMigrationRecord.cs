using NodaTime;

namespace Email.Outbox.Persistence.Migrations.Models
{
    internal sealed class EmailOutboxMigrationRecord
    {
        public long MigrationId { get; set; }
        public Instant AppliedUtc { get; set; }
        public Instant? DeletedUtc { get; set; }
    }
}
