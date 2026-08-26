using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Time.Serialization
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

            return UtcTime.TruncateToPersistencePrecision(parsed);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var utcValue = value.Kind switch
            {
                DateTimeKind.Utc => UtcTime.TruncateToPersistencePrecision(value),
                DateTimeKind.Unspecified => UtcTime.TruncateToPersistencePrecision(UtcDateTime.RestoreFromDatabase(value)),
                _ => throw new JsonException("Local DateTime values cannot be serialized as UTC. Convert the value explicitly first.")
            };

            writer.WriteStringValue(utcValue.ToString("O", CultureInfo.InvariantCulture));
        }
    }
}
