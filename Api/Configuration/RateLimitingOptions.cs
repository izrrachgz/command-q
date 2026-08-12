namespace Api.Configuration
{
    internal class RateLimitingOptions
    {
        public int PermitLimit { get; set; } = 4;
        public int WindowSeconds { get; set; } = 16;
        public int QueueLimit { get; set; } = 2;
    }

    internal sealed class FixedRateLimitingOptions : RateLimitingOptions
    {
    }

    internal sealed class ConcurrencyRateLimitingOptions : RateLimitingOptions
    {
    }

    internal sealed class SlidingRateLimitingOptions : RateLimitingOptions
    {
        public int SegmentsPerWindow { get; set; } = 4;
    }

    internal sealed class TokenBucketRateLimitingOptions : RateLimitingOptions
    {
        public int TokenLimit { get; set; } = 4;
        public int TokensPerPeriod { get; set; } = 4;
    }
}
