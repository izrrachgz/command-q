using System.Text.Json.Serialization;

namespace Email.Outbox.Serialization
{
    [JsonSourceGenerationOptions(
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        WriteIndented = false)]
    [JsonSerializable(typeof(EmailMessageSnapshot))]
    internal sealed partial class EmailOutboxJsonContext : JsonSerializerContext
    {
    }
}
