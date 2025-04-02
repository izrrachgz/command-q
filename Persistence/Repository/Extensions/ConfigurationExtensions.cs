#nullable enable
using Common.Enumerations;
using Application.Constants;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository.Extensions
{
    public static class ConfigurationExtensions
    {
        public static string GetConnectionString(this IConfiguration configuration, string? pathToKey = null)
        {
            string key = pathToKey ?? ApplicationConfiguration.ConnectionStringKeyName;

            return configuration.GetConnectionString(key) ?? string.Empty;
        }

        public static DatabaseEngine GetDatabaseEngine(this IConfiguration configuration, string? pathToKey = null)
        {
            string key = pathToKey ?? ApplicationConfiguration.EngineKeyName;

            return configuration.GetValue<DatabaseEngine>(key);
        }
    }
}
