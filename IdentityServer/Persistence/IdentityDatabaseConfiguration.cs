using IdentityServer.Constants;
using IdentityServer.Localization;
using IdentityServer.Resources;

namespace IdentityServer.Persistence
{
    internal sealed record IdentityDatabaseOptions(IdentityDatabaseEngine Engine, string ConnectionString);

    internal static class IdentityDatabaseConfiguration
    {
        public static IdentityDatabaseOptions GetIdentityDatabaseOptions(this IConfiguration configuration)
        {
            string? configuredEngine = configuration[IdentityServerConfigKeys.ConnectionStrings.Engine];

            if (!Enum.TryParse(configuredEngine, true, out IdentityDatabaseEngine engine) ||
                !Enum.IsDefined(engine))
            {
                string supportedEngines = string.Join(", ", Enum.GetNames<IdentityDatabaseEngine>());

                throw new InvalidOperationException(
                    IdentityServerText.Format(
                        IdentityServerMessages.Configuration_DatabaseEngineMustBeOneOf,
                        IdentityServerConfigKeys.FullPath(IdentityServerConfigKeys.ConnectionStrings.Engine),
                        supportedEngines));
            }

            string connectionString = configuration.GetConnectionString(IdentityServerConfigKeys.ConnectionStrings.IdentityDb)
                ?? throw new InvalidOperationException(RequiredConnectionString());

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(RequiredConnectionString());
            }

            return new IdentityDatabaseOptions(engine, connectionString);
        }

        private static string RequiredConnectionString()
        {
            return IdentityServerText.Format(
                IdentityServerMessages.Configuration_Required,
                $"{IdentityServerConfigKeys.FullPath(IdentityServerConfigKeys.ConnectionStrings.Section)}:{IdentityServerConfigKeys.ConnectionStrings.IdentityDb}");
        }
    }
}
