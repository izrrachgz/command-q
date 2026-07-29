namespace Localization.Models
{
    /// <summary>
    /// Text provider implementations available to the localization service registration.
    /// </summary>
    public enum LocalizationProviderType
    {
        /// <summary>
        /// Uses embedded .resx resources through ResourceManager.
        /// </summary>
        Resx = 1,

        /// <summary>
        /// Uses JSON locale files loaded from the configured locale folder.
        /// </summary>
        Json = 2
    }
}
