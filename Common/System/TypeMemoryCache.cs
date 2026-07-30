using Common.Interfaces;
using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace Common.System
{
    /// <summary>
    /// Type-scoped memory cache that tracks its own keys so clear operations do not affect other cache types.
    /// </summary>
    /// <typeparam name="TItem">Value type stored in this cache scope.</typeparam>
    public sealed class TypeMemoryCache<TItem>(IMemoryCache cache) : ITypeCache<TItem>
    {
        private readonly ConcurrentDictionary<string, byte> _keys = [];

        private static string GetKey(string key) => $@"typed-cache:{typeof(TItem).FullName}:{key}";

        public bool TryGet(string key, out TItem item)
        {
            return cache.TryGetValue(GetKey(key), out item!);
        }

        public void Set(string key, TItem item, TimeSpan slidingExpiration, TimeSpan? absoluteExpiration = null)
        {
            _keys[key] = 0;

            cache.Set(GetKey(key), item, new MemoryCacheEntryOptions
            {
                SlidingExpiration = slidingExpiration,
                AbsoluteExpirationRelativeToNow = absoluteExpiration
            });
        }

        public void Remove(string key)
        {
            _keys.TryRemove(key, out _);
            cache.Remove(GetKey(key));
        }

        public void Clear()
        {
            foreach (var key in _keys.Keys)
                Remove(key);
        }
    }
}
