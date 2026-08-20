using System.Text.Json;
using IdentityServer.Serialization;

namespace IdentityServer.Test.Serialization
{
    public sealed class UtcDateTimeJsonConverterTest
    {
        [Fact]
        public void Serialize_Should_Normalize_Database_DateTime_To_Z()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new UtcDateTimeJsonConverter());
            var value = new DateTime(2026, 8, 20, 18, 30, 15, DateTimeKind.Unspecified);

            var json = JsonSerializer.Serialize(value, options);

            Assert.EndsWith("Z\"", json, StringComparison.Ordinal);
        }
    }
}
