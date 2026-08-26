using NodaTime;

namespace Time.Test
{
    public sealed class UtcTimeTest
    {
        [Fact]
        public void Persistence_Format_Should_Truncate_And_Roundtrip_Microseconds()
        {
            var value = Instant.FromUtc(2026, 8, 25, 12, 30, 15) + Duration.FromNanoseconds(123456789);
            var expected = Instant.FromUtc(2026, 8, 25, 12, 30, 15) + Duration.FromNanoseconds(123456000);

            string text = UtcTime.FormatForPersistence(value);
            Instant result = UtcTime.ParseFromPersistence(text);

            Assert.Equal("2026-08-25 12:30:15.123456Z", text);
            Assert.Equal(expected, result);
            Assert.EndsWith("Z", text, StringComparison.Ordinal);
        }

        [Fact]
        public void Persistence_Precision_Should_Truncate_Toward_The_Start_Of_Time()
        {
            Instant value = Instant.FromUnixTimeTicks(-1);

            Instant result = UtcTime.TruncateToPersistencePrecision(value);

            Assert.Equal(Instant.FromUnixTimeTicks(-10), result);
        }

        [Fact]
        public void UtcDateTime_Should_Reject_NonUtc_Writes_And_Restore_Unspecified_Database_Values()
        {
            var unspecified = new DateTime(2026, 8, 25, 12, 30, 15, DateTimeKind.Unspecified);
            var local = new DateTime(2026, 8, 25, 12, 30, 15, DateTimeKind.Local);

            Assert.Throws<InvalidOperationException>(() => UtcDateTime.Require(unspecified));
            Assert.Equal(DateTimeKind.Utc, UtcDateTime.RestoreFromDatabase(unspecified).Kind);
            Assert.Throws<InvalidOperationException>(() => UtcDateTime.RestoreFromDatabase(local));
        }
    }
}
