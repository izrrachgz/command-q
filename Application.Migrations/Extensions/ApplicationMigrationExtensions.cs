using Microsoft.Extensions.Hosting;
using Application.Migrations.Services;
using Application.Migrations.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Application.Migrations.Extensions
{
    /// <summary>
    /// Registers and executes the application-owned database migrations.
    /// </summary>
    public static class ApplicationMigrationExtensions
    {
        public static IServiceCollection AddApplicationMigrations(this IServiceCollection services)
        {
            services.TryAddSingleton<IMigrationScriptSource, EmbeddedMigrationScriptSource>();
            services.TryAddScoped<ApplicationMigrationRunner>();

            return services;
        }

        public static async Task RunApplicationMigrations(
            this IHost host,
            CancellationToken cancellationToken = default)
        {
            using var scope = host.Services.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<ApplicationMigrationRunner>();

            await runner.RunAsync(cancellationToken);
        }
    }
}
