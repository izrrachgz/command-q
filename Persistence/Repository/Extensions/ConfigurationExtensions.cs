using Common.Enumerations;
using Microsoft.Extensions.Configuration;
using Persistence.Constant;

namespace Persistence.Repository.Extensions
{
    public static class ConfigurationExtensions
    {
        public static string GetDatabaseConnection(this IConfiguration configuration, string? pathToKey = null)
        {
            string key = pathToKey ?? ConfigKeys.ConnectionStrings.Database;

            return configuration.GetConnectionString(key) ?? string.Empty;
        }

        public static DatabaseEngine GetDatabaseEngine(this IConfiguration configuration, string? pathToKey = null)
        {
            string key = pathToKey ?? ConfigKeys.ConnectionStrings.Engine;

            return configuration.GetValue<DatabaseEngine>(key);
        }
    }
}
