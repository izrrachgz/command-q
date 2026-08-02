using Microsoft.EntityFrameworkCore;
using IdentityServer.Persistence.Providers;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentityServer.Persistence
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

        internal static class ConnectionStrings
        {
            public const string PgSql =
                "Host=localhost;Database=command_q_identity;Username=postgres;Password=design-time-only";

            public const string MsSql =
                "Server=localhost;Database=command_q_identity;User ID=design-time;Password=design-time-only;TrustServerCertificate=True";

            public const string MySql =
                "Server=localhost;Database=command_q_identity;User ID=design-time;Password=design-time-only";

            public const string SqLite = "Data Source=command_q_identity.db";
        }
    }

    internal sealed class PgSqlIdentityServerDbContextFactory
        : IDesignTimeDbContextFactory<PgSqlIdentityServerDbContext>
    {
        public PgSqlIdentityServerDbContext CreateDbContext(string[] args)
        {
            return IdentityServerDbContextFactory.Create<PgSqlIdentityServerDbContext>(
                options => options.UseNpgsql(IdentityServerDbContextFactory.ConnectionStrings.PgSql),
                options => new PgSqlIdentityServerDbContext(options));
        }
    }

    internal sealed class MsSqlIdentityServerDbContextFactory
        : IDesignTimeDbContextFactory<MsSqlIdentityServerDbContext>
    {
        public MsSqlIdentityServerDbContext CreateDbContext(string[] args)
        {
            return IdentityServerDbContextFactory.Create<MsSqlIdentityServerDbContext>(
                options => options.UseSqlServer(IdentityServerDbContextFactory.ConnectionStrings.MsSql),
                options => new MsSqlIdentityServerDbContext(options));
        }
    }

    internal sealed class MySqlIdentityServerDbContextFactory
        : IDesignTimeDbContextFactory<MySqlIdentityServerDbContext>
    {
        public MySqlIdentityServerDbContext CreateDbContext(string[] args)
        {
            return IdentityServerDbContextFactory.Create<MySqlIdentityServerDbContext>(
                options => options.UseMySQL(IdentityServerDbContextFactory.ConnectionStrings.MySql),
                options => new MySqlIdentityServerDbContext(options));
        }
    }

    internal sealed class SqLiteIdentityServerDbContextFactory
        : IDesignTimeDbContextFactory<SqLiteIdentityServerDbContext>
    {
        public SqLiteIdentityServerDbContext CreateDbContext(string[] args)
        {
            return IdentityServerDbContextFactory.Create<SqLiteIdentityServerDbContext>(
                options => options.UseSqlite(IdentityServerDbContextFactory.ConnectionStrings.SqLite),
                options => new SqLiteIdentityServerDbContext(options));
        }
    }
}
