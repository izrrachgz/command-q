using Dapper;
using System.Data;
using System.Globalization;

namespace Persistence.Repository.TypeHandlers
{
    internal sealed class SqliteUtcDateTimeTypeHandler : SqlMapper.TypeHandler<DateTime>
    {
        internal const string Format = "yyyy-MM-dd HH:mm:ss.fffffff'Z'";

        public static SqliteUtcDateTimeTypeHandler Instance { get; } = new();

        private SqliteUtcDateTimeTypeHandler()
        {
        }

        public override DateTime Parse(object value)
        {
            if (value is DateTime dateTime)
                return Normalize(dateTime);

            if (value is string text && DateTime.TryParseExact(
                text,
                Format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsed))
            {
                return parsed;
            }

            throw new DataException($"Cannot convert {value.GetType().Name} to a UTC {nameof(DateTime)}.");
        }

        public override void SetValue(IDbDataParameter parameter, DateTime value)
        {
            parameter.DbType = DbType.String;
            parameter.Value = FormatUtc(value);
        }

        internal static string FormatUtc(DateTime value)
        {
            return Normalize(value).ToString(Format, CultureInfo.InvariantCulture);
        }

        private static DateTime Normalize(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }
    }
}
