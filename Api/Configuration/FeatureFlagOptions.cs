namespace Api.Configuration
{
    /// <summary>
    /// Configuration values that control optional application behavior.
    /// </summary>
    public sealed class FeatureFlagOptions
    {
        /// <summary>
        /// Gets the case-sensitive feature name and enabled-state map.
        /// </summary>
        public Dictionary<string, bool> Values { get; init; } = [];
    }
}
