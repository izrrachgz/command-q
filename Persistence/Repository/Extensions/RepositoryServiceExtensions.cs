using Persistence.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Repository.Extensions
{
    /// <summary>
    /// Registers repository services for configured database access.
    /// </summary>
    public static class RepositoryServiceExtensions
    {
        /// <summary>
        /// Registers read-only and writable repositories using the configured database connection and engine.
        /// </summary>
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IWritableRepository>(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var logger = serviceProvider.GetRequiredService<ILogger<WritableRepository>>();
                var environment = serviceProvider.GetService<IHostEnvironment>();
                var engine = configuration.GetDatabaseEngine();
                string migrationsPath = RepositoryFactory.GetMigrationsPath(engine, configuration.GetMigrationsRootPath(), environment?.ContentRootPath);

                return new WritableRepository(configuration.GetDatabaseConnection(), engine, migrationsPath, logger);
            });

            services.AddScoped<IReadOnlyRepository>(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var logger = serviceProvider.GetRequiredService<ILogger<ReadOnlyRepository>>();

                return new ReadOnlyRepository(configuration.GetDatabaseConnection(), configuration.GetDatabaseEngine(), logger);
            });
        }
    }
}
