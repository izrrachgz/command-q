using System.Text.Json.Serialization;

namespace Localization.Serialization
{
    /// <summary>
    /// Source-generated JSON metadata for localization file serialization.
    /// </summary>
    [JsonSerializable(typeof(Dictionary<string, string>))]
    internal partial class LocalizationJsonContext : JsonSerializerContext
    {
    }
}
