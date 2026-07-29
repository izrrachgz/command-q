using System.Text.Json;
using Localization.Models;
using Localization.Abstractions;
using System.Collections.Concurrent;

namespace Localization.Providers
{
    /// <summary>
    /// Resolves localized text from JSON locale files with a shared cache per culture and locale path.
    /// </summary>
    public class JsonTextProvider : ITextProvider
    {
        private static readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> Cache = [];

        private readonly string _culture;

        private readonly string _fallbackCulture;

        private readonly string _localesPath;

        /// <summary>
        /// Creates a JSON provider for the specified culture using default localization options.
        /// </summary>
        /// <param name="culture">Primary culture used to resolve JSON locale values.</param>
        public JsonTextProvider(string culture) : this(culture, new LocalizationOptions())
        {
        }

        /// <summary>
        /// Creates a JSON provider for the specified culture and localization options.
        /// </summary>
        /// <param name="culture">Primary culture used to resolve JSON locale values.</param>
        /// <param name="options">Localization options containing fallback culture and JSON locale path.</param>
        public JsonTextProvider(string culture, LocalizationOptions options)
        {
            _culture = NormalizeCulture(culture);
            _fallbackCulture = NormalizeCulture(options.FallbackCulture);
            _localesPath = options.JsonLocalesPath;
        }

        /// <summary>
        /// Gets a localized JSON value by key, falling back to the configured fallback culture when needed.
        /// </summary>
        public string Get(string key, params object[] args)
        {
            var value = GetValue(_culture, key);

            if (value is null && !_fallbackCulture.Equals(_culture, StringComparison.OrdinalIgnoreCase))
                value = GetValue(_fallbackCulture, key);

            if (value is null)
                return key;

            return args.Length > 0 ? string.Format(value, args) : value;
        }

        private string? GetValue(string culture, string key)
        {
            var values = Cache.GetOrAdd($@"{_localesPath}|{culture}", _ => Load(culture));

            return values.TryGetValue(key, out var value) ? value : null;
        }

        private IReadOnlyDictionary<string, string> Load(string culture)
        {
            var path = Path.Combine(_localesPath, $@"{culture}.json");

            if (!File.Exists(path))
                return new Dictionary<string, string>();

            var json = File.ReadAllText(path);

            return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
        }

        private static string NormalizeCulture(string culture)
        {
            return culture.Split('-')[0];
        }
    }
}
