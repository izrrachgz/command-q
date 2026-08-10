using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Persistence.Providers
{
    public sealed class MySqlIdentityServerDbContext(DbContextOptions<MySqlIdentityServerDbContext> options)
        : IdentityServerDbContext(options)
    {
    }
}
