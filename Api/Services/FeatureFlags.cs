using Api.Abstractions;
using Api.Configuration;
using Microsoft.Extensions.Options;

namespace Api.Services
{
    /// <summary>
    /// Resolves feature flags from the latest monitored configuration snapshot.
    /// </summary>
    internal sealed class FeatureFlags(IOptionsMonitor<FeatureFlagOptions> options) : IFeatureFlags
    {
        /// <inheritdoc />
        public bool IsEnabled(string name)
        {
            return options.CurrentValue.Values.TryGetValue(name, out bool enabled) && enabled;
        }
    }
}
