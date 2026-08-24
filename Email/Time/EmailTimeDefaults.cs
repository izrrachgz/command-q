using NodaTime;
using NodaTime.Text;

namespace Email.Time
{
    /// <summary>
    /// Defines the UTC timezone and persistence pattern shared by the email stack.
    /// </summary>
    public static class EmailTimeDefaults
    {
        /// <summary>
        /// Gets the canonical UTC timezone used by email delivery and outbox processing.
        /// </summary>
        public static DateTimeZone TimeZone { get; } = DateTimeZone.Utc;

        /// <summary>
        /// Gets the fixed-width UTC pattern used for timezone-neutral database text values.
        /// </summary>
        public static InstantPattern PersistencePattern { get; } = InstantPattern.CreateWithInvariantCulture(
            "uuuu-MM-dd HH:mm:ss.fffffff'Z'");
    }
}
