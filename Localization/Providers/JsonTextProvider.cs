using System.Text.Json;
using Localization.Configuration;
using Localization.Abstractions;
using Localization.Serialization;
using System.Collections.Concurrent;

namespace Localization.Providers
{
    /// <summary>
    /// Resolves localized text from JSON locale files with a shared cache per culture and locale path.
    /// </summary>
    public class JsonTextProvider : ITextProvider
    {
        private static readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> Cache = [];

        private readonly string _primaryCultureName;

        private readonly string _fallbackCultureName;

        private readonly string _localesPath;

        /// <summary>
        /// Creates a JSON provider for the specified culture using default localization options.
        /// </summary>
        /// <param name="primaryCultureName">Primary culture used to resolve JSON locale values.</param>
        public JsonTextProvider(string primaryCultureName) : this(primaryCultureName, new LocalizationOptions())
        {
        }

        /// <summary>
        /// Creates a JSON provider for the specified culture and localization options.
        /// </summary>
        /// <param name="primaryCultureName">Primary culture used to resolve JSON locale values.</param>
        /// <param name="localizationOptions">Localization options containing fallback culture and JSON locale path.</param>
        public JsonTextProvider(string primaryCultureName, LocalizationOptions localizationOptions)
        {
            _primaryCultureName = NormalizeCulture(primaryCultureName);
            _fallbackCultureName = NormalizeCulture(localizationOptions.FallbackCulture);
            _localesPath = localizationOptions.JsonLocalesPath;
        }

        /// <summary>
        /// Gets a localized JSON value by key, falling back to the configured fallback culture when needed.
        /// </summary>
        public string Get(string key, params object[] args)
        {
            var value = GetValue(_primaryCultureName, key);

            if (value is null && !_fallbackCultureName.Equals(_primaryCultureName, StringComparison.OrdinalIgnoreCase))
                value = GetValue(_fallbackCultureName, key);

            if (value is null)
                return key;

            return args.Length > 0 ? string.Format(value, args) : value;
        }

        private string? GetValue(string cultureName, string key)
        {
            var values = Cache.GetOrAdd($@"{_localesPath}|{cultureName}", _ => Load(cultureName));

            return values.TryGetValue(key, out var value) ? value : null;
        }

        private IReadOnlyDictionary<string, string> Load(string cultureName)
        {
            var path = Path.Combine(_localesPath, $@"{cultureName}.json");

            if (!File.Exists(path))
                return new Dictionary<string, string>();

            var json = File.ReadAllText(path);

            return JsonSerializer.Deserialize(json, LocalizationJsonContext.Default.DictionaryStringString) ?? [];
        }

        private static string NormalizeCulture(string culture)
        {
            return culture.Split('-')[0];
        }
    }
}
