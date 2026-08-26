using NodaTime;
using NodaTime.Text;

namespace Time
{
    /// <summary>
    /// Defines the canonical UTC timezone and text representation shared by backend projects.
    /// </summary>
    public static class UtcTime
    {
        private const long TicksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000;
        private static readonly InstantPattern PersistencePattern = InstantPattern.CreateWithInvariantCulture(
            PersistencePatternText);

        /// <summary>
        /// Number of fractional-second digits preserved by every backend database provider.
        /// </summary>
        public const int PersistencePrecisionDigits = 6;

        /// <summary>
        /// Fixed-width UTC pattern used for timezone-neutral persistence text values.
        /// </summary>
        public const string PersistencePatternText = "uuuu-MM-dd HH:mm:ss.ffffff'Z'";

        /// <summary>
        /// Canonical UTC timezone used by backend services.
        /// </summary>
        public static DateTimeZone TimeZone { get; } = DateTimeZone.Utc;

        /// <summary>
        /// Truncates an instant to the microsecond precision supported consistently by every database provider.
        /// </summary>
        public static Instant TruncateToPersistencePrecision(Instant value)
        {
            long ticks = value.ToUnixTimeTicks();
            long remainder = ticks % TicksPerMicrosecond;

            if (remainder < 0)
            {
                remainder += TicksPerMicrosecond;
            }

            return Instant.FromUnixTimeTicks(ticks - remainder);
        }

        /// <summary>
        /// Truncates a UTC platform value to the microsecond precision supported consistently by every database provider.
        /// </summary>
        public static DateTime TruncateToPersistencePrecision(DateTime value)
        {
            DateTime utcValue = UtcDateTime.Require(value);
            return new DateTime(utcValue.Ticks - (utcValue.Ticks % TicksPerMicrosecond), DateTimeKind.Utc);
        }

        /// <summary>
        /// Converts an offset value to UTC and truncates it to the shared database precision.
        /// </summary>
        public static DateTimeOffset TruncateToPersistencePrecision(DateTimeOffset value)
        {
            return new DateTimeOffset(TruncateToPersistencePrecision(value.UtcDateTime));
        }

        /// <summary>
        /// Formats an instant using the canonical microsecond UTC persistence representation.
        /// </summary>
        public static string FormatForPersistence(Instant value)
        {
            return PersistencePattern.Format(TruncateToPersistencePrecision(value));
        }

        /// <summary>
        /// Formats a UTC platform value using the canonical microsecond UTC persistence representation.
        /// </summary>
        public static string FormatForPersistence(DateTime value)
        {
            return FormatForPersistence(Instant.FromDateTimeUtc(TruncateToPersistencePrecision(value)));
        }

        /// <summary>
        /// Parses the canonical microsecond UTC persistence representation.
        /// </summary>
        public static Instant ParseFromPersistence(string value)
        {
            ArgumentNullException.ThrowIfNull(value);
            var result = PersistencePattern.Parse(value);

            if (result.Success)
            {
                return result.Value;
            }

            throw new FormatException("The value is not a valid UTC persistence timestamp.", result.Exception);
        }

        /// <summary>
        /// Parses the canonical microsecond UTC persistence representation as a UTC platform value.
        /// </summary>
        public static DateTime ParseDateTimeFromPersistence(string value)
        {
            return ParseFromPersistence(value).ToDateTimeUtc();
        }
    }
}
