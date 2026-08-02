namespace IdentityServer.Logging
{
    internal static partial class StartupLog
    {
        [LoggerMessage(EventId = IdentityServerEventIds.StartupSetting, Level = LogLevel.Information, Message = "{SettingName}: {SettingValue}.")]
        public static partial void StartupSetting(this ILogger logger, string settingName, string? settingValue);

        [LoggerMessage(EventId = IdentityServerEventIds.RunningInDevelopment, Level = LogLevel.Information, Message = "Running in Development mode.")]
        public static partial void RunningInDevelopment(this ILogger logger);
    }
}
