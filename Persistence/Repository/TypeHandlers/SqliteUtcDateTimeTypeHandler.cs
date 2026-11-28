using Time;
using Dapper;
using System.Data;
using System.Globalization;

namespace Persistence.Repository.TypeHandlers
{
    internal sealed class SqliteUtcDateTimeTypeHandler : SqlMapper.TypeHandler<DateTime>
    {
        private const string LegacyUtcFormat = "yyyy-MM-dd HH:mm:ss.FFFFFFF'Z'";

        public static SqliteUtcDateTimeTypeHandler Instance { get; } = new();

        private SqliteUtcDateTimeTypeHandler()
        {
        }

        public override DateTime Parse(object value)
        {
            if (value is DateTime dateTime)
            {
                try
                {
                    DateTime utcValue = UtcDateTime.RestoreFromDatabase(dateTime);
                    return UtcTime.TruncateToPersistencePrecision(utcValue);
                }
                catch (InvalidOperationException exception)
                {
                    throw new DataException("Local DateTime values cannot be materialized as UTC database values.", exception);
                }
            }

            if (value is string text)
            {
                try
                {
                    return UtcTime.ParseDateTimeFromPersistence(text);
                }
                catch (FormatException exception)
                {
                    if (DateTime.TryParseExact(text, LegacyUtcFormat, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var legacyUtc))
                    {
                        return UtcTime.TruncateToPersistencePrecision(legacyUtc);
                    }

                    throw new DataException("The database value is not a valid UTC timestamp.", exception);
                }
            }

            throw new DataException($"Cannot convert {value.GetType().Name} to a UTC {nameof(DateTime)}.");
        }

        public override void SetValue(IDbDataParameter parameter, DateTime value)
        {
            parameter.DbType = DbType.String;
            parameter.Value = FormatUtc(value);
        }

        internal static object NormalizeParameter(object value)
        {
            return value switch
            {
                DateTime dateTime => FormatUtc(dateTime),
                DateTimeOffset dateTimeOffset => FormatUtc(dateTimeOffset.UtcDateTime),
                _ => value
            };
        }

        internal static string FormatUtc(DateTime value)
        {
            return UtcTime.FormatForPersistence(UtcDateTime.Require(value));
        }
    }
}
