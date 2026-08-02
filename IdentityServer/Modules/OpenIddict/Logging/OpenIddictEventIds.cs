using IdentityServer.Logging;

namespace IdentityServer.Modules.OpenIddict.Logging
{
    internal static class OpenIddictEventIds
    {
        public const int DatabaseMigrationStarted = IdentityServerEventIdRanges.OpenIddict + 0;
        public const int DatabaseMigrationCompleted = IdentityServerEventIdRanges.OpenIddict + 1;
    }
}
