namespace Api.Enumerations
{
    /// <summary>
    /// Specifies the available strategies for limiting the rate of access to a resource or service.
    /// </summary>
    /// <remarks>Use this enumeration to select a rate limiting algorithm that best fits the application's
    /// requirements. Each strategy offers different trade-offs in terms of fairness, burst handling, and implementation
    /// complexity. For example, FixedWindow and SlidingWindow are commonly used for simple request quotas, while
    /// TokenBucket allows for controlled bursts, and Concurrent limits the number of simultaneous operations. Choose
    /// the appropriate value based on the desired rate limiting behavior.</remarks>
    public enum RateLimiting
    {
        /// <summary>
        /// Represents a value indicating that no specific option is selected.
        /// </summary>
        None = 0,

        /// <summary>
        /// Represents a fixed window rate limiting strategy, where a set number of requests are allowed within a
        /// specified time window.
        /// </summary>
        /// <remarks>Use this value to enforce rate limits that reset after each fixed interval, such as
        /// allowing a maximum number of requests per minute or hour. This approach is commonly used for simple rate
        /// limiting scenarios where bursts of requests are permitted up to the window limit.</remarks>
        FixedWindow = 1,

        /// <summary>
        /// Represents a rate limiting strategy that allows requests based on a fixed-size sliding time window.
        /// </summary>
        /// <remarks>In the sliding window strategy, the allowed number of requests is distributed over a
        /// moving time interval, providing smoother rate limiting compared to fixed window approaches. This method
        /// helps prevent bursts of traffic at window boundaries and is commonly used to enforce fair usage
        /// policies.</remarks>
        SlidingWindow = 2,

        /// <summary>
        /// Represents a token bucket algorithm used for rate limiting.
        /// </summary>
        /// <remarks>The token bucket algorithm allows for bursts of activity up to a defined capacity
        /// while enforcing an average rate over time. This option is commonly used to control the rate of requests or
        /// operations in distributed systems.</remarks>
        TokenBucket = 3,

        /// <summary>
        /// Represents the concurrent execution mode, allowing multiple operations to be performed simultaneously.
        /// </summary>
        Concurrent = 4,
    }
}
