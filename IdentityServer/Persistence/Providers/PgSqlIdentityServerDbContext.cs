using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Persistence.Providers
{
    public sealed class PgSqlIdentityServerDbContext(
        DbContextOptions<PgSqlIdentityServerDbContext> options)
        : IdentityServerDbContext(options)
    {
    }
}
