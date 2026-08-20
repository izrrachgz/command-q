using Common.Enumerations;
using System.Globalization;

namespace Persistence.Repository.Extensions
{
    public static class DateTimeFilterExtensions
    {
        private const string SqLiteUtcDateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        /// <summary>Converts an instant to the provider-specific value used by UTC datetime predicates.</summary>
        public static object ToDateTimeFilterValue(this DateTimeOffset value, DatabaseEngine engine)
        {
            var utcValue = value.UtcDateTime;

            return engine == DatabaseEngine.SqLite
                ? utcValue.ToString(SqLiteUtcDateTimeFormat, CultureInfo.InvariantCulture)
                : utcValue;
        }
    }
}
