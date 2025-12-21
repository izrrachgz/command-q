#nullable enable
namespace Common.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool NotEmpty(this IEnumerable<object> collection)
        {
            return collection is not null && collection.Any();
        }

        public static bool NotEmpty(this IEnumerable<int> collection)
        {
            return collection is not null && collection.Any();
        }

        public static bool NotEmpty(this IEnumerable<char> collection)
        {
            return collection is not null && collection.Any();
        }

        public static bool NotEmpty(this IEnumerable<string>? collection)
        {
            return collection is not null && collection.Any();
        }

        public static bool NotEmpty(this IEnumerable<float>? collection)
        {
            return collection is not null && collection.Any();
        }

        public static bool Empty<T, U>(this Dictionary<T, U?>? collection) where T : notnull
        {
            if (collection is null)
                return true;

            return collection.Count == 0;
        }

        public static bool Empty(this IEnumerable<int>? collection)
        {
            return collection is null || !collection.Any();
        }

        public static bool Empty(this IEnumerable<string>? collection)
        {
            return collection is null || !collection.Any();
        }
    }
}
