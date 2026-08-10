using IdentityServer.Constants;

namespace IdentityServer.Persistence
{
    internal sealed record IdentityDatabaseSettings(IdentityDatabaseEngine Engine, string ConnectionString);

    internal static class IdentityDatabaseConfiguration
    {
        public static IdentityDatabaseSettings GetIdentityDatabaseSettings(this IConfiguration configuration)
        {
            string? configuredEngine = configuration[ConfigKeys.ConnectionStrings.Engine];

            if (!Enum.TryParse(configuredEngine, true, out IdentityDatabaseEngine engine) ||
                !Enum.IsDefined(engine))
            {
                string supportedEngines = string.Join(", ", Enum.GetNames<IdentityDatabaseEngine>());

                throw new InvalidOperationException(
                    $"{ConfigKeys.ConnectionStrings.Engine} must be one of {supportedEngines}.");
            }

            string connectionString = configuration.GetConnectionString(ConfigKeys.ConnectionStrings.IdentityDb)
                ?? throw new InvalidOperationException("IdentityServer:ConnectionStrings:IdentityDb is required.");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("IdentityServer:ConnectionStrings:IdentityDb is required.");
            }

            return new IdentityDatabaseSettings(engine, connectionString);
        }
    }
}
