namespace Api.Constants
{
    public static class ApiConfiguration
    {
        #region Api

        public const string ApiKeyHeaderKeyName = @"ApiAccessKeys:Key";

        public const string ApiKeyHeaderKeyValue = @"ApiAccessKeys:Value";

        #endregion

        #region JWT

        public const string JwtSecretKey = @"Jwt:SecretKey";

        public const string JwtIssuer = @"Jwt:Issuer";

        public const string JwtAudience = @"Jwt:Audience";

        public const string JwtMinutesToExpire = @"Jwt:MinutesToExpire";

        #endregion

        #region Rate Limiting

        public const string RateLimitingFixedPolicyName = @"FixedWindowRatePolicy";
        public const string RateLimitingSlidingPolicyName = @"SlidingWindowRatePolicy";
        public const string RateLimitingTokenBucketPolicyName = @"TokenBucketRatePolicy";
        public const string RateLimitingConcurrencyPolicyName = @"ConcurrencyRatePolicy";

        public const string RateLimitingFixedOptions = @"RateLimiting:FixedWindow";
        public const string RateLimitingSlidingOptions = @"RateLimiting:Sliding";
        public const string RateLimitingTokenBucketOptions = @"RateLimiting:TokenBucket";
        public const string RateLimitingConcurrencyOptions = @"RateLimiting:Concurrency";

        #endregion
    }
}
