using Localization.Abstractions;

namespace Localization.Providers
{
    public class CompositeTextProvider : ITextProvider
    {
        private readonly IEnumerable<ITextProvider> _providers;

        public CompositeTextProvider(IEnumerable<ITextProvider> providers)
        {
            _providers = providers;
        }

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
