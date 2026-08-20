using System.Text.Json;
using Api.Serialization;

namespace Api.Test.Serialization
{
    public sealed class UtcDateTimeJsonConverterTest
    {
        private static readonly JsonSerializerOptions Options = CreateOptions();

        [Theory]
        [InlineData(DateTimeKind.Utc)]
        [InlineData(DateTimeKind.Unspecified)]
        public void Serialize_Should_Write_Utc_Instant_With_Z(DateTimeKind kind)
        {
            var value = new DateTime(2026, 8, 20, 18, 30, 15, 123, kind);

            var json = JsonSerializer.Serialize(value, Options);

            Assert.EndsWith("Z\"", json, StringComparison.Ordinal);
        }

        [Fact]
        public void Deserialize_Should_Reject_Value_Without_Z()
        {
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DateTime>("\"2026-08-20T18:30:15+00:00\"", Options));
        }

        private static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new UtcDateTimeJsonConverter());
            options.Converters.Add(new UtcDateTimeOffsetJsonConverter());
            return options;
        }

        [Fact]
        public void Serialize_DateTimeOffset_Should_Write_Z()
        {
            var value = new DateTimeOffset(2026, 8, 20, 11, 30, 15, TimeSpan.FromHours(-7));

            var json = JsonSerializer.Serialize(value, Options);

            Assert.EndsWith("Z\"", json, StringComparison.Ordinal);
        }
    }
}
