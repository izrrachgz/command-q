#nullable enable
using System.Net.Mail;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization.Metadata;

namespace Common.Extensions
{
    public static partial class StringsExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        [GeneratedRegex("^\\d+$")]
        private static partial Regex IntegerRegex();

        private static readonly Regex _integerRegex = IntegerRegex();

        public static T? FromJson<T>(this string? content)
        {
            if (content is null)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T?>(content, _jsonSerializerOptions);
        }

        public static T? FromJson<T>(this string? content, JsonTypeInfo<T> jsonTypeInfo)
        {
            if (content is null)
            {
                return default;
            }

            return JsonSerializer.Deserialize(content, jsonTypeInfo);
        }

        public static bool IsNumber(this string content)
        {
            if (string.IsNullOrEmpty(content) || content.Length > 19)
            {
                return false;
            }

            return _integerRegex.IsMatch(content);
        }

        /// <summary>
        /// Determines whether a string is an absolute URI.
        /// </summary>
        /// <param name="content">The URI text to validate.</param>
        /// <returns><see langword="true"/> when the value is an absolute URI; otherwise, <see langword="false"/>.</returns>
        public static bool IsAbsoluteUri(this string? content)
        {
            return Uri.TryCreate(content, UriKind.Absolute, out _);
        }

        public static bool IsEmailAddress(this string? content)
        {
            if (string.IsNullOrEmpty(content))
                return false;
            try
            {
                var email = new MailAddress(content);
                return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Converts a string to a declared enum value.
        /// </summary>
        /// <typeparam name="T">The enum type to parse.</typeparam>
        /// <param name="content">The string representation of the enum value.</param>
        /// <param name="ignoreCase">Whether enum member name matching should ignore case.</param>
        /// <returns>The declared enum value, or <see langword="null"/> when parsing or validation fails.</returns>
        public static T? ToEnum<T>(this string? content, bool ignoreCase = false) where T : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return default;
            }

            if (Enum.TryParse(content, ignoreCase, out T @enum) && Enum.IsDefined(@enum))
            {
                return @enum;
            }

            return default;
        }

        public static string Remove(this string content, string needle)
        {
            return content.Replace(needle, string.Empty);
        }
    }
}
