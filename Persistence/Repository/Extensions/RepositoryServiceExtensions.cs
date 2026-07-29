using Persistence.Interfaces;
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
            services.AddScoped<IWritableRepository, WritableRepository>();
            services.AddScoped<IReadOnlyRepository, ReadOnlyRepository>();
        }
    }
}
