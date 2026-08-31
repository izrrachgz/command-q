using SqlKata;

namespace Persistence.Repository.Extensions
{
    /// <summary>
    /// Provides reusable, whitelisted search and sorting assembly for SqlKata queries.
    /// </summary>
    public static class QuerySearchAndSortExtensions
    {
        /// <summary>
        /// Applies a grouped LIKE search across the supplied columns.
        /// </summary>
        public static Query ApplySearchTerm(
            this Query query,
            string? searchTerm,
            IReadOnlyList<string> columns)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(columns);

            if (string.IsNullOrWhiteSpace(searchTerm) || columns.Count == 0)
                return query;

            var term = $"%{searchTerm.Trim()}%";

            return query.Where(group =>
            {
                for (var index = 0; index < columns.Count; index++)
                {
                    if (index == 0)
                        group.WhereLike(columns[index], term);
                    else
                        group.OrWhereLike(columns[index], term);
                }

                return group;
            });
        }

        /// <summary>
        /// Applies sorting using a map of accepted sort names to database columns.
        /// Unknown sort names use the default column.
        /// </summary>
        /// <param name="defaultAscending">
        /// Optional direction to use when no sort name is supplied. When omitted,
        /// <paramref name="ascendingSort"/> is used.
        /// </param>
        public static Query ApplySorting(
            this Query query,
            string? sortBy,
            bool ascendingSort,
            IReadOnlyDictionary<string, string> columns,
            string defaultColumn,
            bool? defaultAscending = null)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(columns);
            ArgumentException.ThrowIfNullOrWhiteSpace(defaultColumn);

            var hasSortBy = !string.IsNullOrWhiteSpace(sortBy);
            var column = hasSortBy && columns.TryGetValue(sortBy!, out var mappedColumn)
                ? mappedColumn
                : defaultColumn;
            var ascending = hasSortBy || !defaultAscending.HasValue
                ? ascendingSort
                : defaultAscending.Value;

            return ascending
                ? query.OrderBy(column)
                : query.OrderByDesc(column);
        }
    }
}
