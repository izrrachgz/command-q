using Time.Extensions;

namespace Time.Test.Extensions
{
    public sealed class DateTimeUtcExtensionsTest
    {
        [Fact]
        public void ToLocalTime_Should_Convert_Utc_To_Selected_TimeZone()
        {
            var utc = new DateTime(2026, 1, 15, 18, 30, 0, DateTimeKind.Utc);

            var local = utc.ToLocalTime("America/Hermosillo");

            Assert.Equal(new DateTime(2026, 1, 15, 11, 30, 0, DateTimeKind.Unspecified), local);
        }

        [Fact]
        public void ToLocalTime_Should_Convert_DateTimeOffset_Instant_To_Selected_TimeZone()
        {
            var utc = new DateTimeOffset(2026, 1, 15, 18, 30, 0, TimeSpan.Zero);

            var local = utc.ToLocalTime("America/Hermosillo");

            Assert.Equal(new DateTime(2026, 1, 15, 11, 30, 0, DateTimeKind.Unspecified), local);
        }

        [Fact]
        public void ToLocalTime_Should_Reject_NonUtc_DateTime()
        {
            var value = new DateTime(2026, 1, 15, 18, 30, 0, DateTimeKind.Unspecified);

            Assert.Throws<ArgumentException>(() => value.ToLocalTime("America/Hermosillo"));
        }

        [Fact]
        public void ToUtcFromLocalTime_Should_Roundtrip_An_Unambiguous_Value()
        {
            var local = new DateTime(2026, 1, 15, 11, 30, 0, DateTimeKind.Unspecified);

            var utc = local.ToUtcFromLocalTime("America/Hermosillo");

            Assert.Equal(new DateTime(2026, 1, 15, 18, 30, 0, DateTimeKind.Utc), utc);
        }

        [Theory]
        [InlineData(2026, 3, 8, 2, 30)]
        [InlineData(2026, 11, 1, 1, 30)]
        public void ToUtcFromLocalTime_Should_Reject_Skipped_Or_Ambiguous_Value(int year, int month, int day, int hour, int minute)
        {
            var local = new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Unspecified);

            Assert.Throws<ArgumentException>(() => local.ToUtcFromLocalTime("America/New_York"));
        }

        [Theory]
        [InlineData("America/Hermosillo", true)]
        [InlineData("Not/AZone", false)]
        [InlineData("", false)]
        public void IsValidTimeZone_Should_Use_Tzdb_Identifiers(string timeZone, bool expected)
        {
            Assert.Equal(expected, DateTimeUtcExtensions.IsValidTimeZone(timeZone));
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(-7, false)]
        public void IsUtc_Should_Require_Zero_Offset(int offsetHours, bool expected)
        {
            var value = new DateTimeOffset(2026, 1, 15, 11, 30, 0, TimeSpan.FromHours(offsetHours));

            Assert.Equal(expected, DateTimeUtcExtensions.IsUtc(value));
        }
    }
}
