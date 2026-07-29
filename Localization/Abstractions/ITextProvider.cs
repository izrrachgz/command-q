namespace Localization.Abstractions
{
    /// <summary>
    /// Resolves localized text by key and optionally formats it with arguments.
    /// </summary>
    public interface ITextProvider
    {
        /// <summary>
        /// Gets a localized value by key. Returns the key when no provider can resolve it.
        /// </summary>
        /// <param name="key">Resource or locale key to resolve.</param>
        /// <param name="args">Optional values used to format the resolved text.</param>
        string Get(string key, params object[] args);
    }
}
