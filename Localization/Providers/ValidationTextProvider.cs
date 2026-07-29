using Public;
using System.Globalization;
using Localization.Abstractions;

namespace Localization.Providers
{
    /// <summary>
    /// Resolves validation templates and validator field names from embedded .resx resources.
    /// </summary>
    public class ValidationTextProvider : ITextProvider
    {
        private readonly CultureInfo _culture;

        private readonly CultureInfo? _fallbackCulture;

        /// <summary>
        /// Creates a provider using the current UI culture.
        /// </summary>
        public ValidationTextProvider() : this(CultureInfo.CurrentUICulture)
        {
        }

        /// <summary>
        /// Creates a provider for a specific culture and optional fallback culture.
        /// </summary>
        /// <param name="culture">Primary culture used to resolve validation text.</param>
        /// <param name="fallbackCulture">Fallback culture used when the primary culture has no value.</param>
        public ValidationTextProvider(CultureInfo culture, CultureInfo? fallbackCulture = null)
        {
            _culture = culture;
            _fallbackCulture = fallbackCulture;
        }

        /// <summary>
        /// Gets a localized validation template or field name by key.
        /// </summary>
        public string Get(string key, params object[] args)
        {
            var value = GetResourceValue(key, _culture);

            if (value is null && _fallbackCulture is not null && !_fallbackCulture.Equals(_culture))
                value = GetResourceValue(key, _fallbackCulture);

            if (value == null)
                return key;

            return args.Length > 0 ? string.Format(value, args) : value;
        }

        private static string? GetResourceValue(string key, CultureInfo culture)
        {
            return ValidationText.ResourceManager.GetString(key, culture)
                ?? FieldNames.ResourceManager.GetString(key, culture);
        }
    }
}
