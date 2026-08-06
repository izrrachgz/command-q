using Common.Enumerations;
using Persistence.Constant;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository.Extensions
{
    /// <summary>
    /// Reads persistence configuration values used by repositories and command runners.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Gets the configured database connection string.
        /// </summary>
        /// <param name="configuration">Application configuration source.</param>
        /// <param name="pathToKey">Optional connection-string key. Defaults to the main database connection.</param>
        public static string GetDatabaseConnection(this IConfiguration configuration, string? pathToKey = null)
        {
            string key = pathToKey ?? ConfigKeys.ConnectionStrings.Database;

            return configuration.GetConnectionString(key) ??
                configuration[$"ConnectionStrings:{key}"] ??
                configuration[key] ??
                string.Empty;
        }

        /// <summary>
        /// Gets the configured database engine.
        /// </summary>
        /// <param name="configuration">Application configuration source.</param>
        /// <param name="pathToKey">Optional configuration key. Defaults to the main database engine key.</param>
        public static DatabaseEngine GetDatabaseEngine(this IConfiguration configuration, string? pathToKey = null)
        {
            string key = pathToKey ?? ConfigKeys.ConnectionStrings.Engine;

            return configuration.GetValue<DatabaseEngine>(key);
        }

    }
}
