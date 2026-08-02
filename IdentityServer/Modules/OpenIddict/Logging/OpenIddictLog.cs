namespace IdentityServer.Modules.OpenIddict.Logging
{
    internal static partial class OpenIddictLog
    {
        [LoggerMessage(EventId = OpenIddictEventIds.DatabaseMigrationStarted, Level = LogLevel.Information, Message = "Applying pending OpenIddict database migrations.")]
        public static partial void DatabaseMigrationStarted(this ILogger logger);

        [LoggerMessage(EventId = OpenIddictEventIds.DatabaseMigrationCompleted, Level = LogLevel.Information, Message = "OpenIddict database migrations are current.")]
        public static partial void DatabaseMigrationCompleted(this ILogger logger);
    }
}
