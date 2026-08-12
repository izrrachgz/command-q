using Api.Constants;

namespace Api.Middleware.Exceptions
{
    /// <summary>
    /// Indicates that the request does not identify a configured API client.
    /// </summary>
    public sealed class NotValidApiClientException : Exception
    {
        /// <summary>
        /// Creates an exception for a request with an invalid API client.
        /// </summary>
        public NotValidApiClientException() : base(ApiMessages.NotValidApiClient)
        {
        }
    }
}
