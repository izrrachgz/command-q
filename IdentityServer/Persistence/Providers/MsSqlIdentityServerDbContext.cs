using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Persistence.Providers
{
    public sealed class MsSqlIdentityServerDbContext(DbContextOptions<MsSqlIdentityServerDbContext> options)
        : IdentityServerDbContext(options)
    {
    }
}
