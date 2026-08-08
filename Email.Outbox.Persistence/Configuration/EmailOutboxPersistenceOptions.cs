using Common.Enumerations;
using Email.Outbox.Persistence.Constants;

namespace Email.Outbox.Persistence.Configuration
{
    public sealed class EmailOutboxPersistenceOptions
    {
        public DatabaseEngine Engine { get; set; } = DatabaseEngine.SqLite;
        public string ConnectionString { get; set; } = EmailOutboxPersistenceConstants.DefaultConnectionString;
        public bool RunMigrationsOnStartup { get; set; } = true;
    }
}
