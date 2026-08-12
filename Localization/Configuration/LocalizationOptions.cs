using Localization.Models;

namespace Localization.Configuration
{
    /// <summary>
    /// Options that control localization provider order, supported cultures and JSON locale lookup.
    /// </summary>
    public sealed class LocalizationOptions
    {
        /// <summary>
        /// Culture used when the current UI culture does not resolve a value.
        /// </summary>
        public string FallbackCulture { get; set; } = "en-US";

        /// <summary>
        /// Cultures supported by the application.
        /// </summary>
        public string[] SupportedCultures { get; set; } = ["en-US", "es-ES"];

        /// <summary>
        /// Relative or absolute folder containing JSON locale files.
        /// </summary>
        public string JsonLocalesPath { get; set; } = "locales";

        /// <summary>
        /// Provider order used by the composite text provider. JSON is first by default because locale files are cached
        /// per culture/path and then resolved through dictionary lookup.
        /// </summary>
        public LocalizationProviderType[] ProviderOrder { get; set; } = [LocalizationProviderType.Json, LocalizationProviderType.Resx];
    }
}
