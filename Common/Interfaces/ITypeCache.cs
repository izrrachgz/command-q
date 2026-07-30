namespace Common.Interfaces
{
    /// <summary>
    /// Stores cache entries for a specific value type.
    /// </summary>
    /// <typeparam name="TItem">Value type stored in the cache.</typeparam>
    public interface ITypeCache<TItem>
    {
        bool TryGet(string key, out TItem item);

        void Set(string key, TItem item, TimeSpan slidingExpiration, TimeSpan? absoluteExpiration = null);

        void Remove(string key);

        void Clear();
    }
}
