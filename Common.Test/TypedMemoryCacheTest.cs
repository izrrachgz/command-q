using Common.System;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace Common.Test;

public sealed class TypedMemoryCacheTest
{
    [Fact]
    public void Clear_Should_Skip_A_Key_That_Has_Already_Been_Evicted()
    {
        using var memory = new TrackingMemoryCache();
        var cache = new TypedMemoryCache<string>(memory, NullLogger<TypedMemoryCache<string>>.Instance);

        cache.Set("old", "value", TimeSpan.FromMinutes(1));
        var eviction = memory.CaptureEviction();
        memory.EvictLastEntry();
        eviction();

        cache.Clear();
        Assert.Empty(memory.RemoveCalls);
    }

    [Fact]
    public void An_Older_Eviction_Should_Not_Untrack_A_Replacement()
    {
        using var memory = new TrackingMemoryCache();
        var cache = new TypedMemoryCache<string>(memory, NullLogger<TypedMemoryCache<string>>.Instance);
        var other = new TypedMemoryCache<int>(memory, NullLogger<TypedMemoryCache<int>>.Instance);

        cache.Set("same", "first", TimeSpan.FromMinutes(1));
        var previousEviction = memory.CaptureEviction();
        cache.Set("same", "second", TimeSpan.FromMinutes(1));
        other.Set("same", 3, TimeSpan.FromMinutes(1));
        previousEviction();

        cache.Clear();
        Assert.False(cache.TryGet("same", out _));
        Assert.True(other.TryGet("same", out var unaffected));
        Assert.Equal(3, unaffected);
    }

    private sealed class TrackingMemoryCache : IMemoryCache
    {
        private readonly MemoryCache _cache = new(new MemoryCacheOptions());
        private ICacheEntry? _lastEntry;

        public List<object> RemoveCalls { get; } = [];

        public ICacheEntry CreateEntry(object key) => _lastEntry = _cache.CreateEntry(key);

        public bool TryGetValue(object key, out object? value) => _cache.TryGetValue(key, out value);

        public void Remove(object key)
        {
            RemoveCalls.Add(key);
            _cache.Remove(key);
        }

        public void EvictLastEntry() => _cache.Remove(_lastEntry!.Key);

        public Action CaptureEviction()
        {
            // Capture the callbacks so the tests can control when the notification runs.
            ICacheEntry entry = _lastEntry!;
            var callbacks = entry.PostEvictionCallbacks.ToArray();
            return () =>
            {
                foreach (var callback in callbacks)
                    callback.EvictionCallback!(entry.Key, entry.Value, EvictionReason.Expired, callback.State);
            };
        }

        public void Dispose() => _cache.Dispose();
    }
}
