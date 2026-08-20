using NodaTime;

namespace Application.Extensions
{
    /// <summary>Converts UTC instants to and from account-local wall-clock values using IANA/TZDB timezone identifiers.</summary>
    public static class DateTimeUtcExtensions
    {
        /// <summary>Converts a UTC <see cref="DateTime"/> to the local wall-clock time in the specified timezone.</summary>
        public static DateTime ToAccountTime(this DateTime utcValue, string timeZoneId)
        {
            if (utcValue.Kind != DateTimeKind.Utc)
                throw new ArgumentException("The supplied value must have DateTimeKind.Utc.", nameof(utcValue));

            return Instant.FromDateTimeUtc(utcValue)
                .InZone(GetTimeZone(timeZoneId))
                .ToDateTimeUnspecified();
        }

        /// <summary>Converts a UTC-offset instant to the local wall-clock time in the specified timezone.</summary>
        public static DateTime ToAccountTime(this DateTimeOffset utcValue, string timeZoneId)
        {
            return Instant.FromDateTimeOffset(utcValue.ToUniversalTime())
                .InZone(GetTimeZone(timeZoneId))
                .ToDateTimeUnspecified();
        }

        /// <summary>Converts an unambiguous account-local wall-clock value to UTC.</summary>
        /// <exception cref="ArgumentException">The value is not unspecified, or it is ambiguous or skipped in the timezone.</exception>
        public static DateTime ToUtcFromAccountTime(this DateTime localValue, string timeZoneId)
        {
            if (localValue.Kind != DateTimeKind.Unspecified)
                throw new ArgumentException("The supplied wall-clock value must have DateTimeKind.Unspecified.", nameof(localValue));

            var localDateTime = LocalDateTime.FromDateTime(localValue);

            try
            {
                return GetTimeZone(timeZoneId).AtStrictly(localDateTime).ToInstant().ToDateTimeUtc();
            }
            catch (AmbiguousTimeException exception)
            {
                throw new ArgumentException("The supplied wall-clock value is ambiguous in the selected timezone.", nameof(localValue), exception);
            }
            catch (SkippedTimeException exception)
            {
                throw new ArgumentException("The supplied wall-clock value does not exist in the selected timezone.", nameof(localValue), exception);
            }
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
