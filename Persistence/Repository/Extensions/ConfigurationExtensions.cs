using Common.Enumerations;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository.Extensions
{
    public static class ConfigurationExtensions
    {
        public static string GetDatabaseConnection(this IConfiguration configuration, string? pathToKey = null)
        {
            string key = pathToKey ?? @"Db";

            return configuration.GetConnectionString(key) ?? string.Empty;
        }

        public static DatabaseEngine GetDatabaseEngine(this IConfiguration configuration, string? pathToKey = null)
        {
            string key = pathToKey ?? @"ConnectionStrings:Engine";

            return configuration.GetValue<DatabaseEngine>(key);
        }
    }
}
