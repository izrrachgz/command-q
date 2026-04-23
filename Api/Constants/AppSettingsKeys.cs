namespace Api.Constants
{
    internal sealed class AppSettingsKeys
    {
        #region Api

        public const string ApiClientsKeyName = @"ApiClients";

        #endregion

        #region JWT

        public const string JwtSecretKey = @"Jwt:SecretKey";

        public const string JwtIssuer = @"Jwt:Issuer";

        public const string JwtAudience = @"Jwt:Audience";

        public const string JwtMinutesToExpire = @"Jwt:MinutesToExpire";

        #endregion

        #region Cookie

        public const string CookieName = @"Cookie:Name";
        public const string CookieDaysToExpire = @"Cookie:DaysToExpire";

        #endregion


        #region Rate Limiting

        public const string RateLimitingFixedPolicyName = @"FixedWindowRatePolicy";
        public const string RateLimitingSlidingPolicyName = @"SlidingWindowRatePolicy";
        public const string RateLimitingTokenBucketPolicyName = @"TokenBucketRatePolicy";
        public const string RateLimitingConcurrencyPolicyName = @"ConcurrencyRatePolicy";

        public const string RateLimitingType = @"RateLimiting:Type";

        public const string RateLimitingFixedOptions = @"RateLimiting:FixedWindow";
        public const string RateLimitingSlidingOptions = @"RateLimiting:Sliding";
        public const string RateLimitingTokenBucketOptions = @"RateLimiting:TokenBucket";
        public const string RateLimitingConcurrencyOptions = @"RateLimiting:Concurrency";

        #endregion


        #region Cache

        public const string CacheSettingsMaxAgeSeconds = @"CacheSettings:MaxAgeSeconds";

        #endregion

        #region Database

        public const string DatabaseEngine = @"ConnectionStrings:Engine";

        #endregion

        #region Health Check

        public const string HealthCheckUrl = @"HealthCheck:UrlCheckInternetAccess";

        #endregion
    }
}
