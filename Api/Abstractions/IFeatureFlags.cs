namespace Api.Abstractions
{
    /// <summary>
    /// Provides access to configuration-backed feature flags.
    /// </summary>
    public interface IFeatureFlags
    {
        /// <summary>
        /// Determines whether a named feature is currently enabled.
        /// </summary>
        /// <param name="name">The configured feature name.</param>
        /// <returns><see langword="true"/> when the feature is enabled; otherwise, <see langword="false"/>.</returns>
        bool IsEnabled(string name);
    }
}
