using Localization.Abstractions;

namespace Localization.Providers
{
    /// <summary>
    /// Resolves text by trying multiple providers in configured order.
    /// </summary>
    public class CompositeTextProvider(IEnumerable<ITextProvider> providers) : ITextProvider
    {
        private readonly IReadOnlyList<ITextProvider> _providers = [.. providers];

        /// <summary>
        /// Gets the first localized value returned by the configured providers.
        /// </summary>
        public string Get(string key, params object[] args)
        {
            foreach (var provider in _providers)
            {
                var value = provider.Get(key, args);
                if (value != key)
                    return value;
            }
            return key;
        }
    }
}
