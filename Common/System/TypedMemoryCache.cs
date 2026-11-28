using Common.Logging;
using Common.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace Common.System
{
    /// <summary>
    /// Type-scoped memory cache that tracks its own keys so clear operations do not affect other cache types.
    /// </summary>
    /// <typeparam name="TEntry">Value type stored in this cache scope.</typeparam>
    public sealed class TypedMemoryCache<TEntry>(IMemoryCache memoryCache, ILogger<TypedMemoryCache<TEntry>> logger) : ITypedCache<TEntry>
    {
        private readonly object _gate = new();
        private readonly Dictionary<string, object> _keys = [];

        private static string GetKey(string key) => $@"typed-cache:{typeof(TEntry).FullName}:{key}";

        public bool TryGet(string key, out TEntry entry)
        {
            bool found = memoryCache.TryGetValue(GetKey(key), out entry!);

            logger.TypedCacheLookup(typeof(TEntry).Name, found ? "hit" : "miss");

            return found;
        }

        public void Set(string key, TEntry entry, TimeSpan slidingExpiration, TimeSpan? absoluteExpiration = null)
        {
            ArgumentNullException.ThrowIfNull(key);

            var registration = new object();
            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = slidingExpiration,
                AbsoluteExpirationRelativeToNow = absoluteExpiration
            };
            options.RegisterPostEvictionCallback((_, _, _, _) => Untrack(key, registration));

            lock (_gate)
            {
                memoryCache.Set(GetKey(key), entry, options);
                _keys[key] = registration;
            }

            logger.TypedCacheSet(typeof(TEntry).Name);
        }

        public void Remove(string key)
        {
            ArgumentNullException.ThrowIfNull(key);

            lock (_gate)
            {
                memoryCache.Remove(GetKey(key));
                _keys.Remove(key);
            }

            logger.TypedCacheRemoved(typeof(TEntry).Name);
        }

        public void Clear()
        {
            int count;
            lock (_gate)
            {
                var keys = _keys.Keys.ToArray();
                count = keys.Length;
                foreach (string key in keys)
                {
                    memoryCache.Remove(GetKey(key));
                    _keys.Remove(key);
                }
            }

            logger.TypedCacheCleared(typeof(TEntry).Name, count);
        }

        private void Untrack(string key, object registration)
        {
            lock (_gate)
            {
                // A delayed notification may refer to a version that Set has since replaced.
                if (_keys.TryGetValue(key, out var current) && ReferenceEquals(registration, current))
                    _keys.Remove(key);
            }
        }
    }
}
