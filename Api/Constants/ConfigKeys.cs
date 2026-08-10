namespace Api.Constants
{
    internal static class ConfigKeys
    {
        public const string Section = "Api";
        public const string Language = "Language";

        public static class Api
        {
            public const string Clients = @"ApiClients";
        }

        public static class Authentication
        {
            public const string Section = @"Authentication";
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
            public const string Section = @"CacheSettings";
            public const string MaxAgeSeconds = @"CacheSettings:MaxAgeSeconds";
        }

        public static class Database
        {
            public const string Section = @"ConnectionStrings";
            public const string Engine = @"ConnectionStrings:Engine";
        }

        public static class Migrations
        {
            public const string RunOnStartup = @"Migrations:RunOnStartup";
        }

        public static class EmailOutbox
        {
            public const string Section = "EmailOutbox";
            public const string PersistenceSection = "Persistence";
            public const string Engine = "EmailOutbox:Persistence:Engine";
        }

        public static class HealthCheck
        {
            public const string Url = @"HealthCheck:UrlCheckInternetAccess";
        }
    }
}
