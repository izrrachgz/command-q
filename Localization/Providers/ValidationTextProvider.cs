using Localization.Abstractions;

namespace Localization.Providers
{
    /// <summary>
    /// Compatibility facade for validators. Application-owned validation and field-name
    /// resources are resolved by the configured generic text provider.
    /// </summary>
    public sealed class ValidationTextProvider : ITextProvider
    {
        private readonly ITextProvider _textProvider;

        /// <summary>
        /// Creates an unconfigured provider that returns missing keys unchanged.
        /// This is intended for validator unit tests; applications should resolve the provider from DI.
        /// </summary>
        public ValidationTextProvider() : this(new EmptyTextProvider())
        {
        }

        public ValidationTextProvider(ITextProvider textProvider)
        {
            _textProvider = textProvider;
        }

        /// <summary>
        /// Gets a localized validation template or field name by key.
        /// </summary>
        public string Get(string key, params object[] args)
        {
            return _textProvider.Get(key, args);
        }

        private sealed class EmptyTextProvider : ITextProvider
        {
            public string Get(string key, params object[] args) => key;
        }
    }
}
