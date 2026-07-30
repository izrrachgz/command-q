using Microsoft.Extensions.Logging;

namespace Common.Logging
{
    /// <summary>
    /// Source-generated logging messages used by typed cache implementations.
    /// </summary>
    internal static partial class TypedMemoryCacheLog
    {
        [LoggerMessage(EventId = CommonEventIds.TypedCacheLookup, Level = LogLevel.Debug, Message = "Typed cache {CacheType} {Result} for key {CacheKey}.")]
        public static partial void TypedCacheLookup(this ILogger logger, string cacheType, string result, string cacheKey);

        [LoggerMessage(EventId = CommonEventIds.TypedCacheSet, Level = LogLevel.Debug, Message = "Typed cache {CacheType} set key {CacheKey}.")]
        public static partial void TypedCacheSet(this ILogger logger, string cacheType, string cacheKey);

        [LoggerMessage(EventId = CommonEventIds.TypedCacheRemoved, Level = LogLevel.Debug, Message = "Typed cache {CacheType} removed key {CacheKey}.")]
        public static partial void TypedCacheRemoved(this ILogger logger, string cacheType, string cacheKey);

        [LoggerMessage(EventId = CommonEventIds.TypedCacheCleared, Level = LogLevel.Debug, Message = "Typed cache {CacheType} cleared {Count} keys.")]
        public static partial void TypedCacheCleared(this ILogger logger, string cacheType, int count);
    }
}

