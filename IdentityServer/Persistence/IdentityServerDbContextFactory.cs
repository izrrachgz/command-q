using IdentityServer.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentityServer.Persistence
{
    internal sealed class IdentityServerDbContextFactory : IDesignTimeDbContextFactory<IdentityServerDbContext>
    {
        public IdentityServerDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<IdentityServerDbContextFactory>()
                .AddEnvironmentVariables()
                .Build();

            string connectionString = configuration.GetConnectionString(ConfigKeys.ConnectionStrings.IdentityDb)
                ?? throw new InvalidOperationException("ConnectionStrings:IdentityDb is required for design-time operations.");

            var options = new DbContextOptionsBuilder<IdentityServerDbContext>();

            options.UseSqlServer(connectionString, sqlServer => sqlServer.EnableRetryOnFailure());
            options.UseOpenIddict();

            return new IdentityServerDbContext(options.Options);
        }
    }
}
