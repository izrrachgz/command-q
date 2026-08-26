using NodaTime;

namespace Time.Extensions
{
    /// <summary>
    /// Provides NodaTime conversions for the injectable platform time provider.
    /// </summary>
    public static class TimeProviderExtensions
    {
        /// <summary>
        /// Gets the provider's current UTC value as an unambiguous NodaTime instant.
        /// </summary>
        public static Instant GetCurrentInstant(this TimeProvider timeProvider)
        {
            ArgumentNullException.ThrowIfNull(timeProvider);
            return UtcTime.TruncateToPersistencePrecision(Instant.FromDateTimeOffset(timeProvider.GetUtcNow()));
        }

        /// <summary>
        /// Gets the provider's current UTC value as a platform value at the shared persistence precision.
        /// </summary>
        public static DateTime GetCurrentUtcDateTime(this TimeProvider timeProvider)
        {
            ArgumentNullException.ThrowIfNull(timeProvider);
            return UtcTime.TruncateToPersistencePrecision(timeProvider.GetUtcNow().UtcDateTime);
        }
    }
}
