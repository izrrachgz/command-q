using Microsoft.EntityFrameworkCore;
using IdentityServer.Persistence.Providers;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentityServer.Persistence.DesignTime
{
    internal sealed class SqLiteIdentityServerDbContextFactory
        : IDesignTimeDbContextFactory<SqLiteIdentityServerDbContext>
    {
        public SqLiteIdentityServerDbContext CreateDbContext(string[] args)
        {
            return IdentityServerDbContextFactory.Create<SqLiteIdentityServerDbContext>(
                options => options.UseSqlite(
                    IdentityServerDbContextFactory.DesignTimePlaceholderConnectionStrings.SqLite),
                options => new SqLiteIdentityServerDbContext(options));
        }
    }
}
