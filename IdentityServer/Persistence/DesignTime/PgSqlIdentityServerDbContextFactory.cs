using Microsoft.EntityFrameworkCore;
using IdentityServer.Persistence.Providers;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentityServer.Persistence.DesignTime
{
    internal sealed class PgSqlIdentityServerDbContextFactory
        : IDesignTimeDbContextFactory<PgSqlIdentityServerDbContext>
    {
        public PgSqlIdentityServerDbContext CreateDbContext(string[] args)
        {
            return IdentityServerDbContextFactory.Create<PgSqlIdentityServerDbContext>(
                options => options.UseNpgsql(
                    IdentityServerDbContextFactory.DesignTimePlaceholderConnectionStrings.PgSql),
                options => new PgSqlIdentityServerDbContext(options));
        }
    }
}
