namespace Api.Configuration
{
    /// <summary>Contains shared limits for a time-window rate limiting policy.</summary>
    internal class RateLimitingOptions
    {
        /// <summary>Gets or sets the number of requests allowed during one window.</summary>
        public int PermitLimit { get; set; } = 4;
        /// <summary>Gets or sets the window duration in seconds.</summary>
        public int WindowSeconds { get; set; } = 16;
        /// <summary>Gets or sets the number of excess requests allowed to wait for a permit.</summary>
        public int QueueLimit { get; set; } = 2;
    }

    /// <summary>Configures the fixed-window policy used by infrequent operations.</summary>
    internal sealed class FixedRateLimitingOptions : RateLimitingOptions
    {
    }

    /// <summary>Configures the instance-global concurrency policy used by expensive operations.</summary>
    internal sealed class ConcurrencyRateLimitingOptions
    {
        /// <summary>Gets or sets the number of requests that may execute concurrently.</summary>
        public int PermitLimit { get; set; } = 4;
        /// <summary>Gets or sets the number of excess requests allowed to wait for a permit.</summary>
        public int QueueLimit { get; set; } = 2;
    }

    /// <summary>Configures the sliding-window policy used by administrative operations.</summary>
    internal sealed class SlidingRateLimitingOptions : RateLimitingOptions
    {
        /// <summary>Gets or sets the number of segments used to smooth requests across the window.</summary>
        public int SegmentsPerWindow { get; set; } = 4;
    }

    /// <summary>Configures the token-bucket policy used by ordinary SPA traffic.</summary>
    internal sealed class TokenBucketRateLimitingOptions
    {
        /// <summary>Gets or sets the token replenishment period in seconds.</summary>
        public int WindowSeconds { get; set; } = 16;
        /// <summary>Gets or sets the number of excess requests allowed to wait for a token.</summary>
        public int QueueLimit { get; set; } = 2;
        /// <summary>Gets or sets the maximum burst capacity.</summary>
        public int TokenLimit { get; set; } = 4;
        /// <summary>Gets or sets the tokens restored during each replenishment period.</summary>
        public int TokensPerPeriod { get; set; } = 4;
    }
}
