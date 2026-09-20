using Localization.Abstractions;

namespace Localization.Providers
{
    /// <summary>
    /// Resolves text by trying multiple providers in configured order.
    /// </summary>
    public sealed class CompositeTextProvider(IEnumerable<ITextProvider> providers) : ITextProvider
    {
        private readonly IReadOnlyList<ITextProvider> _providers = [.. providers];

        /// <summary>
        /// Gets the first localized value returned by the configured providers.
        /// </summary>
        public string Get(string key, params object[] args)
        {
            foreach (var provider in _providers)
            {
                if (provider is ResxTextProvider resx && resx.TryGet(key, out var resxValue))
                    return args.Length > 0 ? string.Format(resxValue!, args) : resxValue!;

                if (provider is JsonTextProvider json && json.TryGet(key, out var jsonValue))
                    return args.Length > 0 ? string.Format(jsonValue!, args) : jsonValue!;

                var value = provider.Get(key, args);
                if (!string.Equals(value, key, StringComparison.Ordinal))
                    return value;
            }
            return key;
        }
    }
}
