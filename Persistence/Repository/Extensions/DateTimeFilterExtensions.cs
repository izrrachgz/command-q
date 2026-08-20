using SqlKata;
using Common.Enumerations;
using Common.Models.Request;
using Persistence.Repository.TypeHandlers;

namespace Persistence.Repository.Extensions
{
    public static class DateTimeFilterExtensions
    {
        /// <summary>Converts an instant to the provider-specific value used by UTC datetime predicates.</summary>
        public static object ToDateTimeFilterValue(this DateTimeOffset value, DatabaseEngine engine)
        {
            var utcValue = value.UtcDateTime;

            return engine == DatabaseEngine.SqLite
                ? SqliteUtcDateTimeTypeHandler.FormatUtc(utcValue)
                : utcValue;
        }

        /// <summary>Applies an inclusive UTC range to the selected datetime column.</summary>
        public static Query ApplyUtcRange(this Query query, string column, SearchCriteria criteria, DatabaseEngine engine)
        {
            if (criteria.StartDate.HasValue)
                query.Where(column, ">=", criteria.StartDate.Value.ToDateTimeFilterValue(engine));

            if (criteria.EndDate.HasValue)
                query.Where(column, "<=", criteria.EndDate.Value.ToDateTimeFilterValue(engine));

            return query;
        }
    }
}
