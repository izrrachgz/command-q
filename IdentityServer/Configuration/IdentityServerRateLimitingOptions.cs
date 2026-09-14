namespace IdentityServer.Configuration
{
    /// <summary>Configures host-level rate limiting for IdentityServer workloads.</summary>
    internal sealed class IdentityServerRateLimitingOptions
    {
        /// <summary>Gets or sets whether IdentityServer applies configured rate-limit policies.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Gets or sets the fixed-window policy for credential-bearing operations.</summary>
        public FixedWindowRateLimitOptions Authentication { get; set; } = new();

        /// <summary>Gets or sets the sliding-window policy for confirmation and recovery challenges.</summary>
        public SlidingWindowRateLimitOptions Challenges { get; set; } = new();

        /// <summary>Gets or sets the token-bucket policy for ordinary protocol and account traffic.</summary>
        public TokenBucketRateLimitOptions Default { get; set; } = new();

        /// <summary>Gets or sets the sliding-window policy for internal administrative operations.</summary>
        public SlidingWindowRateLimitOptions Administration { get; set; } = new();
    }

    /// <summary>Configures a fixed-window rate-limit policy.</summary>
    internal sealed class FixedWindowRateLimitOptions
    {
        /// <summary>Gets or sets the permits available during each window.</summary>
        public int PermitLimit { get; set; } = 10;

        /// <summary>Gets or sets the window duration in seconds.</summary>
        public int WindowSeconds { get; set; } = 60;

        /// <summary>Gets or sets the number of requests allowed to wait for a permit.</summary>
        public int QueueLimit { get; set; }
    }

    /// <summary>Configures a sliding-window rate-limit policy.</summary>
    internal sealed class SlidingWindowRateLimitOptions
    {
        /// <summary>Gets or sets the permits available during each rolling window.</summary>
        public int PermitLimit { get; set; } = 120;

        /// <summary>Gets or sets the rolling-window duration in seconds.</summary>
        public int WindowSeconds { get; set; } = 60;

        /// <summary>Gets or sets the number of segments used to smooth window rollover.</summary>
        public int SegmentsPerWindow { get; set; } = 6;

        /// <summary>Gets or sets the number of requests allowed to wait for a permit.</summary>
        public int QueueLimit { get; set; } = 4;
    }

    /// <summary>Configures a token-bucket rate-limit policy.</summary>
    internal sealed class TokenBucketRateLimitOptions
    {
        /// <summary>Gets or sets the maximum burst capacity.</summary>
        public int TokenLimit { get; set; } = 120;

        /// <summary>Gets or sets the tokens restored during each replenishment period.</summary>
        public int TokensPerPeriod { get; set; } = 60;

        /// <summary>Gets or sets the replenishment period in seconds.</summary>
        public int ReplenishmentPeriodSeconds { get; set; } = 10;

        /// <summary>Gets or sets the number of requests allowed to wait for a token.</summary>
        public int QueueLimit { get; set; } = 12;
    }
}
