using System.Globalization;
using System.Resources;
using Localization.Abstractions;

namespace Localization.Providers
{
    /// <summary>
    /// Resolves localized text from application-owned .resx resources.
    /// </summary>
    public sealed class ResxTextProvider : ITextProvider
    {
        private readonly IReadOnlyList<ResourceManager> _resourceManagers;
        private readonly CultureInfo _primaryCulture;
        private readonly CultureInfo? _fallbackCulture;

        /// <summary>
        /// Creates a provider for a specific culture and optional fallback culture.
        /// </summary>
        /// <param name="culture">Primary culture used to resolve messages.</param>
        /// <param name="fallbackCulture">Fallback culture used when the primary culture has no value.</param>
        public ResxTextProvider(
            IEnumerable<ResourceManager> resourceManagers,
            CultureInfo primaryCulture,
            CultureInfo? fallbackCulture = null)
        {
            ArgumentNullException.ThrowIfNull(resourceManagers);
            _resourceManagers = [.. resourceManagers];
            _primaryCulture = primaryCulture;
            _fallbackCulture = fallbackCulture;
        }

        /// <summary>
        /// Gets a localized operation message by key.
        /// </summary>
        public string Get(string key, params object[] args)
        {
            if (!TryGet(key, out var value))
                return key;

            return args.Length > 0 ? string.Format(value!, args) : value!;
        }

        internal bool TryGet(string key, out string? value)
        {
            value = GetResourceValue(key, _primaryCulture);

            if (value is null && _fallbackCulture is not null && !_fallbackCulture.Equals(_primaryCulture))
                value = GetResourceValue(key, _fallbackCulture);

            return value is not null;
        }

        private string? GetResourceValue(string key, CultureInfo culture)
        {
            foreach (var resourceManager in _resourceManagers)
            {
                var value = resourceManager.GetString(key, culture);
                if (value is not null)
                    return value;
            }

            return null;
        }
    }
}
