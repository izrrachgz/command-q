using System.Text.Json;
using Localization.Abstractions;

namespace Localization.Providers
{
    public class JsonTextProvider : ITextProvider
    {
        private Dictionary<string, string> _cache = [];

        public JsonTextProvider(string culture)
        {
            Load(culture);
        }

        private void Load(string culture)
        {
            var path = Path.Combine("locales", $"{culture}.json");

            if (!File.Exists(path)) return;

            var json = File.ReadAllText(path);

            _cache = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
        }

        public string Get(string key, params object[] args)
        {
            if (!_cache.TryGetValue(key, out var value))
                return key;

            return args.Length > 0 ? string.Format(value, args) : value;
        }
    }
}
