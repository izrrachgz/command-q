using Microsoft.Extensions.Logging;

namespace Api.Logging
{
    /// <summary>
    /// Source-generated logging messages for reloadable runtime configuration.
    /// </summary>
    internal static partial class RuntimeConfigurationLog
    {
        [LoggerMessage(EventId = ApiEventIds.ConfigurationReloaded, Level = LogLevel.Information, Message = "Runtime configuration reloaded. Section: {Section}. Details: {Details}.")]
        public static partial void ConfigurationReloaded(this ILogger logger, string section, string details);
    }
}
