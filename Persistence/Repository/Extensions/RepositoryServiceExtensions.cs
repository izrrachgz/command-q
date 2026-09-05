using Persistence.Abstractions;
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
        /// Registers separate scoped read-only and writable repositories using the configured engine.
        /// </summary>
        public static void AddRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            string connectionString = configuration.GetDatabaseConnection();
            var engine = configuration.GetDatabaseEngine();

            services.AddScoped<IWritableRepository>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<WritableRepository>>();

                return new WritableRepository(connectionString, engine, logger);
            });

            services.AddScoped<IReadOnlyRepository>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<ReadOnlyRepository>>();

                return new ReadOnlyRepository(connectionString, engine, logger);
            });
        }
    }
}
