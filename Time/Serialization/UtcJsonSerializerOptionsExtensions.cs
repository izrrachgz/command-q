using System.Text.Json;

namespace Time.Serialization
{
    /// <summary>
    /// Adds strict UTC converters to JSON serialization options.
    /// </summary>
    public static class UtcJsonSerializerOptionsExtensions
    {
        public static JsonSerializerOptions AddUtcDateTimeConverters(this JsonSerializerOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            if (!options.Converters.Any(converter => converter is UtcDateTimeJsonConverter))
            {
                options.Converters.Add(new UtcDateTimeJsonConverter());
            }

            if (!options.Converters.Any(converter => converter is UtcDateTimeOffsetJsonConverter))
            {
                options.Converters.Add(new UtcDateTimeOffsetJsonConverter());
            }

            return options;
        }
    }
}
