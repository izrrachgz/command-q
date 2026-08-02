using Microsoft.EntityFrameworkCore;
using IdentityServer.Modules.OpenIddict;
using IdentityServer.Modules.OpenIddict.Logging;

namespace IdentityServer.Persistence
{
    internal static class MigrationExtensions
    {
        public static async Task RunIdentityServerMigrations(this WebApplication app)
        {
            app.Logger.DatabaseMigrationStarted();

            await using var scope = app.Services.CreateAsyncScope();
            var database = scope.ServiceProvider.GetRequiredService<IdentityServerDbContext>();

            await database.Database.MigrateAsync();

            app.Logger.DatabaseMigrationCompleted();
        }

        public static async Task SeedIdentityServer(this WebApplication app)
        {
            await using var scope = app.Services.CreateAsyncScope();
            var seeder = scope.ServiceProvider.GetRequiredService<OpenIddictSeeder>();

            await seeder.Seed();
        }
    }
}
