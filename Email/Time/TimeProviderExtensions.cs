using NodaTime;

namespace Email.Time
{
    /// <summary>
    /// Provides NodaTime conversions for the injectable platform time provider.
    /// </summary>
    public static class TimeProviderExtensions
    {
        /// <summary>
        /// Gets the provider's current UTC value as an unambiguous NodaTime instant.
        /// </summary>
        /// <param name="timeProvider">The provider that owns the current time.</param>
        /// <returns>The current instant.</returns>
        public static Instant GetCurrentInstant(this TimeProvider timeProvider)
        {
            ArgumentNullException.ThrowIfNull(timeProvider);
            return Instant.FromDateTimeOffset(timeProvider.GetUtcNow());
        }
    }
}
