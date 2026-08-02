using Microsoft.EntityFrameworkCore;
using IdentityServer.Persistence.Providers;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentityServer.Persistence.DesignTime
{
    internal sealed class MySqlIdentityServerDbContextFactory
        : IDesignTimeDbContextFactory<MySqlIdentityServerDbContext>
    {
        public MySqlIdentityServerDbContext CreateDbContext(string[] args)
        {
            return IdentityServerDbContextFactory.Create<MySqlIdentityServerDbContext>(
                options => options.UseMySQL(
                    IdentityServerDbContextFactory.DesignTimePlaceholderConnectionStrings.MySql),
                options => new MySqlIdentityServerDbContext(options));
        }
    }
}
