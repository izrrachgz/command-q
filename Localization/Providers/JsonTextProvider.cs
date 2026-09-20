using System.Text.Json;
using System.Globalization;
using Localization.Configuration;
using Localization.Abstractions;
using Localization.Serialization;
using System.Collections.Concurrent;

namespace Localization.Providers
{
    /// <summary>
    /// Resolves localized text from JSON locale files with a shared cache per culture and locale path.
    /// </summary>
    public sealed class JsonTextProvider : ITextProvider
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
            _localesPath = Path.IsPathRooted(localizationOptions.JsonLocalesPath)
                ? localizationOptions.JsonLocalesPath
                : Path.Combine(AppContext.BaseDirectory, localizationOptions.JsonLocalesPath);
        }

        /// <summary>
        /// Gets a localized JSON value by key, falling back to the configured fallback culture when needed.
        /// </summary>
        public string Get(string key, params object[] args)
        {
            if (!TryGet(key, out var value))
                return key;

            return args.Length > 0 ? string.Format(value!, args) : value!;
        }

        internal bool TryGet(string key, out string? value)
        {
            value = GetValue(_primaryCultureName, key);

            if (value is null && !_fallbackCultureName.Equals(_primaryCultureName, StringComparison.OrdinalIgnoreCase))
                value = GetValue(_fallbackCultureName, key);

            return value is not null;
        }

        private string? GetValue(string cultureName, string key)
        {
            var values = Cache.GetOrAdd($@"{_localesPath}|{cultureName}", _ => Load(cultureName));

            return values.TryGetValue(key, out var value) ? value : null;
        }

        private IReadOnlyDictionary<string, string> Load(string cultureName)
        {
            var path = ResolvePath(cultureName);

            if (path is null)
                return new Dictionary<string, string>();

            var json = File.ReadAllText(path);

            return JsonSerializer.Deserialize(json, LocalizationJsonContext.Default.DictionaryStringString) ?? [];
        }

        private string? ResolvePath(string cultureName)
        {
            var exactPath = Path.Combine(_localesPath, $@"{cultureName}.json");
            if (File.Exists(exactPath))
                return exactPath;

            var neutralCulture = CultureInfo.GetCultureInfo(cultureName).TwoLetterISOLanguageName;
            var neutralPath = Path.Combine(_localesPath, $@"{neutralCulture}.json");
            return File.Exists(neutralPath) ? neutralPath : null;
        }

        private static string NormalizeCulture(string culture)
        {
            return CultureInfo.GetCultureInfo(culture).Name;
        }
    }
}
