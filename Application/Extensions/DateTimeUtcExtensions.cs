using NodaTime;

namespace Application.Extensions
{
    /// <summary>Converts UTC instants to and from account-local wall-clock values using IANA/TZDB timezone identifiers.</summary>
    public static class DateTimeUtcExtensions
    {
        /// <summary>Converts a UTC <see cref="DateTime"/> to the local wall-clock time in the specified timezone.</summary>
        public static DateTime ToAccountTime(this DateTime utcValue, string timeZoneId)
        {
            DateTime normalizedUtc = utcValue.Kind switch
            {
                DateTimeKind.Utc => utcValue,
                DateTimeKind.Local => utcValue.ToUniversalTime(),
                _ => DateTime.SpecifyKind(utcValue, DateTimeKind.Utc)
            };

            return Instant.FromDateTimeUtc(normalizedUtc)
                .InZone(GetTimeZone(timeZoneId))
                .ToDateTimeUnspecified();
        }

        /// <summary>Converts an account-local wall-clock value to UTC, resolving DST gaps and ambiguity leniently.</summary>
        public static DateTime ToUtcFromAccountTime(this DateTime localValue, string timeZoneId)
        {
            var localDateTime = LocalDateTime.FromDateTime(DateTime.SpecifyKind(localValue, DateTimeKind.Unspecified));
            return GetTimeZone(timeZoneId).AtLeniently(localDateTime).ToInstant().ToDateTimeUtc();
        }

        /// <summary>Returns whether the supplied identifier exists in NodaTime's TZDB provider.</summary>
        public static bool IsValidTimeZone(string? timeZoneId)
        {
            return !string.IsNullOrWhiteSpace(timeZoneId) && DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId.Trim()) is not null;
        }

        private static DateTimeZone GetTimeZone(string timeZoneId)
        {
            return DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId?.Trim() ?? string.Empty)
                ?? throw new ArgumentException($"The timezone '{timeZoneId}' is not a valid TZDB identifier.", nameof(timeZoneId));
        }
    }
}
