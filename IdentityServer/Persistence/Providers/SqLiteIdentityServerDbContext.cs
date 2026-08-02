using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Persistence.Providers
{
    public sealed class SqLiteIdentityServerDbContext(
        DbContextOptions<SqLiteIdentityServerDbContext> options)
        : IdentityServerDbContext(options)
    {
    }
}
