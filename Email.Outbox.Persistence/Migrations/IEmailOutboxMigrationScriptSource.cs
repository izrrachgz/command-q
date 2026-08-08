using Common.Enumerations;
using Email.Outbox.Persistence.Migrations.Models;

namespace Email.Outbox.Persistence.Migrations
{
    internal interface IEmailOutboxMigrationScriptSource
    {
        Task<IReadOnlyList<EmailOutboxMigrationScript>> GetScriptsAsync(
            DatabaseEngine engine,
            CancellationToken cancellationToken = default);
    }
}
