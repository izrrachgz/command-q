using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IdentityServer.Serialization
{
    internal sealed class UtcDateTimeJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            if (string.IsNullOrWhiteSpace(value) || !value.EndsWith('Z') || !DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var parsed) || parsed.Kind != DateTimeKind.Utc)
            {
                throw new JsonException("UTC datetime values must be ISO-8601 strings ending in Z.");
            }

            return parsed;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var utcValue = value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
                _ => value.ToUniversalTime()
            };

            writer.WriteStringValue(utcValue.ToString("O", CultureInfo.InvariantCulture));
        }
    }
}
