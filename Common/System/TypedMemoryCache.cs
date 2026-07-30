using Common.Interfaces;
using Common.Logging;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace Common.System
{
    /// <summary>
    /// Type-scoped memory cache that tracks its own keys so clear operations do not affect other cache types.
    /// </summary>
    /// <typeparam name="TEntry">Value type stored in this cache scope.</typeparam>
    public sealed class TypedMemoryCache<TEntry>(IMemoryCache memoryCache, ILogger<TypedMemoryCache<TEntry>> logger) : ITypedCache<TEntry>
    {
        private readonly ConcurrentDictionary<string, byte> _keys = [];

        private static string GetKey(string key) => $@"typed-cache:{typeof(TEntry).FullName}:{key}";

        public bool TryGet(string key, out TEntry entry)
        {
            bool found = memoryCache.TryGetValue(GetKey(key), out entry!);

            logger.TypedCacheLookup(typeof(TEntry).Name, found ? "hit" : "miss", key);

            return found;
        }

        public void Set(string key, TEntry entry, TimeSpan slidingExpiration, TimeSpan? absoluteExpiration = null)
        {
            _keys[key] = 0;

            memoryCache.Set(GetKey(key), entry, new MemoryCacheEntryOptions
            {
                SlidingExpiration = slidingExpiration,
                AbsoluteExpirationRelativeToNow = absoluteExpiration
            });

            logger.TypedCacheSet(typeof(TEntry).Name, key);
        }

        public void Remove(string key)
        {
            _keys.TryRemove(key, out _);
            memoryCache.Remove(GetKey(key));

            logger.TypedCacheRemoved(typeof(TEntry).Name, key);
        }

        public void Clear()
        {
            int count = _keys.Count;

            foreach (var key in _keys.Keys)
                Remove(key);

            logger.TypedCacheCleared(typeof(TEntry).Name, count);
        }
    }
}
