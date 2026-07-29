using Public;
using System.Globalization;
using Localization.Abstractions;

namespace Localization.Providers
{
    /// <summary>
    /// Resolves operation messages from embedded .resx resources.
    /// </summary>
    public class ResxTextProvider : ITextProvider
    {
        private readonly CultureInfo _culture;

        private readonly CultureInfo? _fallbackCulture;

        /// <summary>
        /// Creates a provider using the current UI culture.
        /// </summary>
        public ResxTextProvider() : this(CultureInfo.CurrentUICulture)
        {
        }

        /// <summary>
        /// Creates a provider for a specific culture and optional fallback culture.
        /// </summary>
        /// <param name="culture">Primary culture used to resolve messages.</param>
        /// <param name="fallbackCulture">Fallback culture used when the primary culture has no value.</param>
        public ResxTextProvider(CultureInfo culture, CultureInfo? fallbackCulture = null)
        {
            _culture = culture;
            _fallbackCulture = fallbackCulture;
        }

        /// <summary>
        /// Gets a localized operation message by key.
        /// </summary>
        public string Get(string key, params object[] args)
        {
            var value = Messages.ResourceManager.GetString(key, _culture);

            if (value is null && _fallbackCulture is not null && !_fallbackCulture.Equals(_culture))
                value = Messages.ResourceManager.GetString(key, _fallbackCulture);

            if (value == null)
                return key;

            return args.Length > 0 ? string.Format(value, args) : value;
        }
    }
}
