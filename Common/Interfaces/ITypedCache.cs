namespace Common.Interfaces
{
    /// <summary>
    /// Stores cache entries for a specific value type.
    /// </summary>
    /// <typeparam name="TEntry">Value type stored in the cache.</typeparam>
    public interface ITypedCache<TEntry>
    {
        bool TryGet(string key, out TEntry entry);

        void Set(string key, TEntry entry, TimeSpan slidingExpiration, TimeSpan? absoluteExpiration = null);

        void Remove(string key);

        void Clear();
    }
}
