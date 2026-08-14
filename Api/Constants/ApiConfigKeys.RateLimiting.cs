namespace Api.Constants
{
    internal static partial class ApiConfigKeys
    {
        public static class RateLimiting
        {
            public const string FixedPolicyName = @"FixedWindowRatePolicy";
            public const string SlidingPolicyName = @"SlidingWindowRatePolicy";
            public const string TokenBucketPolicyName = @"TokenBucketRatePolicy";
            public const string ConcurrencyPolicyName = @"ConcurrencyRatePolicy";

            public const string Type = @"RateLimiting:Type";
            public const string FixedOptions = @"RateLimiting:FixedWindow";
            public const string SlidingOptions = @"RateLimiting:Sliding";
            public const string TokenBucketOptions = @"RateLimiting:TokenBucket";
            public const string ConcurrencyOptions = @"RateLimiting:Concurrency";
        }
    }
}
