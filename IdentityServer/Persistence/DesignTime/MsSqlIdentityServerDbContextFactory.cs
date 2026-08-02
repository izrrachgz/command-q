using Microsoft.EntityFrameworkCore;
using IdentityServer.Persistence.Providers;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentityServer.Persistence.DesignTime
{
    internal sealed class MsSqlIdentityServerDbContextFactory
        : IDesignTimeDbContextFactory<MsSqlIdentityServerDbContext>
    {
        public MsSqlIdentityServerDbContext CreateDbContext(string[] args)
        {
            return IdentityServerDbContextFactory.Create<MsSqlIdentityServerDbContext>(
                options => options.UseSqlServer(
                    IdentityServerDbContextFactory.DesignTimePlaceholderConnectionStrings.MsSql),
                options => new MsSqlIdentityServerDbContext(options));
        }
    }
}
