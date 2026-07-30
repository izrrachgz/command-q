using Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace Common.System
{
    /// <summary>
    /// Type-scoped memory cache that tracks its own keys so clear operations do not affect other cache types.
    /// </summary>
    /// <typeparam name="TItem">Value type stored in this cache scope.</typeparam>
    public sealed class TypeMemoryCache<TItem>(IMemoryCache cache, ILogger<TypeMemoryCache<TItem>> logger) : ITypeCache<TItem>
    {
        private readonly ConcurrentDictionary<string, byte> _keys = [];

        private static string GetKey(string key) => $@"typed-cache:{typeof(TItem).FullName}:{key}";

        public bool TryGet(string key, out TItem item)
        {
            bool found = cache.TryGetValue(GetKey(key), out item!);

            logger.LogDebug(@"Typed cache {CacheType} {Result} for key {CacheKey}.", typeof(TItem).Name, found ? "hit" : "miss", key);

            return found;
        }

        public void Set(string key, TItem item, TimeSpan slidingExpiration, TimeSpan? absoluteExpiration = null)
        {
            _keys[key] = 0;

            cache.Set(GetKey(key), item, new MemoryCacheEntryOptions
            {
                SlidingExpiration = slidingExpiration,
                AbsoluteExpirationRelativeToNow = absoluteExpiration
            });

            logger.LogDebug(@"Typed cache {CacheType} set key {CacheKey}.", typeof(TItem).Name, key);
        }

        public void Remove(string key)
        {
            _keys.TryRemove(key, out _);
            cache.Remove(GetKey(key));

            logger.LogDebug(@"Typed cache {CacheType} removed key {CacheKey}.", typeof(TItem).Name, key);
        }

        public void Clear()
        {
            int count = _keys.Count;

            foreach (var key in _keys.Keys)
                Remove(key);

            logger.LogDebug(@"Typed cache {CacheType} cleared {Count} keys.", typeof(TItem).Name, count);
        }
    }
}
