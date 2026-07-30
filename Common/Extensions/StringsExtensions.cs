#nullable enable
using System.Net.Mail;
using System.Text.Json;
using System.Text.RegularExpressions;

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

        public static bool IsNumber(this string content)
        {
            if (string.IsNullOrEmpty(content) || content.Length > 19)
            {
                return false;
            }

            return _integerRegex.IsMatch(content);
        }

        public static bool IsUri(this string? content)
        {
            if (string.IsNullOrEmpty(content))
                return false;

            try
            {
                var uri = new Uri(content);
                return true;
            }
            catch { }
            return false;
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

        public static T? ToEnum<T>(this string? content) where T : Enum
        {
            return ToEnum<T>(content, false);
        }

        public static T? ToEnum<T>(this string? content, bool ignoreCase = false) where T : Enum
        {
            if (content is null)
                return default;

            if (Enum.TryParse(typeof(T), content, ignoreCase, out var @enum))
                return (T)@enum;

            return default;
        }

        public static string Remove(this string content, string needle)
        {
            return content.Replace(needle, string.Empty);
        }
    }
}
