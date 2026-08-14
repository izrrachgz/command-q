#nullable enable
namespace Common.Extensions
{
    /// <summary>
    /// Provides null-safe collection helpers.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Determines whether a sequence contains at least one item.
        /// </summary>
        /// <typeparam name="T">The sequence item type.</typeparam>
        /// <param name="collection">The sequence to inspect.</param>
        /// <returns><see langword="true"/> when the sequence contains an item; otherwise, <see langword="false"/>.</returns>
        public static bool NotEmpty<T>(this IEnumerable<T>? collection)
        {
            return collection is not null && collection.Any();
        }

        /// <summary>
        /// Determines whether a sequence is null or contains no items.
        /// </summary>
        /// <typeparam name="T">The sequence item type.</typeparam>
        /// <param name="collection">The sequence to inspect.</param>
        /// <returns><see langword="true"/> when the sequence is null or empty; otherwise, <see langword="false"/>.</returns>
        public static bool Empty<T>(this IEnumerable<T>? collection)
        {
            return collection is null || !collection.Any();
        }
    }
}
