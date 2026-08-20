using NodaTime;
using Application.Extensions;
using Application.Modules.Identity.Accounts.Preferences;

namespace Application.Test.Modules.Common.Extensions
{
    public sealed class DateTimeUtcExtensionsTest
    {
        [Fact]
        public void ToAccountTime_Should_Convert_Utc_To_Account_TimeZone()
        {
            var utc = new DateTime(2026, 1, 15, 18, 30, 0, DateTimeKind.Utc);

            var local = utc.ToAccountTime("America/Hermosillo");

            Assert.Equal(new DateTime(2026, 1, 15, 11, 30, 0, DateTimeKind.Unspecified), local);
        }

        [Fact]
        public void ToAccountTime_Should_Convert_DateTimeOffset_Instant_To_Account_TimeZone()
        {
            var utc = new DateTimeOffset(2026, 1, 15, 18, 30, 0, TimeSpan.Zero);

            var local = utc.ToAccountTime("America/Hermosillo");

            Assert.Equal(new DateTime(2026, 1, 15, 11, 30, 0, DateTimeKind.Unspecified), local);
        }

        [Fact]
        public void ToAccountTime_Should_Reject_NonUtc_DateTime()
        {
            var value = new DateTime(2026, 1, 15, 18, 30, 0, DateTimeKind.Unspecified);

            Assert.Throws<ArgumentException>(() => value.ToAccountTime("America/Hermosillo"));
        }

        [Fact]
        public void ToUtcFromAccountTime_Should_Roundtrip_An_Unambiguous_Value()
        {
            var local = new DateTime(2026, 1, 15, 11, 30, 0, DateTimeKind.Unspecified);

            var utc = local.ToUtcFromAccountTime("America/Hermosillo");

            Assert.Equal(new DateTime(2026, 1, 15, 18, 30, 0, DateTimeKind.Utc), utc);
        }

        [Theory]
        [InlineData(2026, 3, 8, 2, 30)]
        [InlineData(2026, 11, 1, 1, 30)]
        public void ToUtcFromAccountTime_Should_Reject_Skipped_Or_Ambiguous_Value(
            int year,
            int month,
            int day,
            int hour,
            int minute)
        {
            var local = new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Unspecified);

            Assert.Throws<ArgumentException>(() => local.ToUtcFromAccountTime("America/New_York"));
        }

        [Theory]
        [InlineData("America/Hermosillo", true)]
        [InlineData("Not/AZone", false)]
        [InlineData("", false)]
        public void IsValidTimeZone_Should_Use_Tzdb_Identifiers(string timeZone, bool expected)
        {
            Assert.Equal(expected, DateTimeUtcExtensions.IsValidTimeZone(timeZone));
        }

        [Fact]
        public void AccountPreference_TimeZone_MaxLength_Should_Accommodate_All_Current_Tzdb_Ids()
        {
            int longestIdentifier = DateTimeZoneProviders.Tzdb.Ids.Max(identifier => identifier.Length);

            Assert.True(longestIdentifier <= AccountPreferenceConstants.TimeZoneMaxLength);
        }
    }
}
