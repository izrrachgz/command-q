namespace Api.Models
{
    public class RateLimitingOptions
    {
        public int PermitLimit { get; set; } = 4;
        public int WindowSeconds { get; set; } = 16;
        public int QueueLimit { get; set; } = 2;
    }

    public sealed class SlidingRateLimitingOptions : RateLimitingOptions
    {
        public int SegmentsPerWindow { get; set; } = 4;
    }

    public sealed class TokenBucketRateLimitingOptions : RateLimitingOptions
    {
        public int TokenLimit { get; set; } = 4;
        public int TokensPerPeriod { get; set; } = 4;
    }
}
