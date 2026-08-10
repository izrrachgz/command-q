namespace Email.Outbox.Persistence.Migrations.Models
{
    internal sealed record EmailOutboxMigrationScript(long MigrationId, string Name, string Content, string Checksum);
}
