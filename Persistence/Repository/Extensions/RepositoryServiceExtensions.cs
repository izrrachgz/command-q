using Persistence.Interfaces;
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
                var engine = configuration.GetDatabaseEngine();

                return new WritableRepository(configuration.GetDatabaseConnection(), engine, logger);
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
