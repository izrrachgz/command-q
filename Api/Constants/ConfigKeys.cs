namespace Api.Constants
{
    internal static class ConfigKeys
    {
        public static class Api
        {
            public const string Clients = @"ApiClients";
        }

        public static class Jwt
        {
            public const string Section = @"Jwt";
            public const string SecretKey = @"Jwt:SecretKey";
            public const string Issuer = @"Jwt:Issuer";
            public const string Audience = @"Jwt:Audience";
            public const string MinutesToExpire = @"Jwt:MinutesToExpire";
        }

        public static class Auth
        {
            public const string Section = @"Auth";
        }

        public static class Cookie
        {
            public const string Name = @"Cookie:Name";
            public const string DaysToExpire = @"Cookie:DaysToExpire";
        }

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

        public static class Cache
        {
            public const string MaxAgeSeconds = @"CacheSettings:MaxAgeSeconds";
        }

        public static class Database
        {
            public const string Engine = @"ConnectionStrings:Engine";
        }

        public static class HealthCheck
        {
            public const string Url = @"HealthCheck:UrlCheckInternetAccess";
        }
    }
}
