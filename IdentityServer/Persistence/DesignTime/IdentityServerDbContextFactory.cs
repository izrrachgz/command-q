using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Persistence.DesignTime
{
    internal static class IdentityServerDbContextFactory
    {
        public static TContext Create<TContext>(
            Action<DbContextOptionsBuilder<TContext>> configureProvider,
            Func<DbContextOptions<TContext>, TContext> createContext)
            where TContext : IdentityServerDbContext
        {
            var options = new DbContextOptionsBuilder<TContext>();

            configureProvider(options);
            options.UseOpenIddict();

            return createContext(options.Options);
        }

        // Migration scaffolding requires parsable provider options without needing a real database target.
        internal static class DesignTimePlaceholderConnectionStrings
        {
            public const string PgSql =
                "Host=design-time.invalid;Database=command_q_identity;Username=design-time;Password=design-time-only";

            public const string MsSql =
                "Server=design-time.invalid;Database=command_q_identity;User ID=design-time;Password=design-time-only;TrustServerCertificate=True";

            public const string MySql =
                "Server=design-time.invalid;Database=command_q_identity;User ID=design-time;Password=design-time-only";

            public const string SqLite = "Data Source=:memory:";
        }
    }
}
