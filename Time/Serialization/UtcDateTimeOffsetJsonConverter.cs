using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Time.Serialization
{
    internal sealed class UtcDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            if (string.IsNullOrWhiteSpace(value) || !value.EndsWith('Z') || !DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var parsed))
            {
                throw new JsonException("UTC datetime values must be ISO-8601 strings ending in Z.");
            }

            return UtcTime.TruncateToPersistencePrecision(parsed);
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            DateTimeOffset utcValue = UtcTime.TruncateToPersistencePrecision(value);
            writer.WriteStringValue(utcValue.UtcDateTime.ToString("O", CultureInfo.InvariantCulture));
        }
    }
}
