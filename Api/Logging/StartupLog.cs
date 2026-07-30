using Microsoft.Extensions.Logging;

namespace Api.Logging
{
    /// <summary>
    /// Source-generated logging messages used while reporting application startup settings.
    /// </summary>
    internal static partial class StartupLog
    {
        [LoggerMessage(EventId = ApiEventIds.StartupSetting, Level = LogLevel.Information, Message = "{SettingName}: {SettingValue}.")]
        public static partial void StartupSetting(this ILogger logger, string settingName, object? settingValue);

        [LoggerMessage(EventId = ApiEventIds.RunningInDevelopment, Level = LogLevel.Information, Message = "Running on Development mode.")]
        public static partial void RunningInDevelopment(this ILogger logger);
    }
}

