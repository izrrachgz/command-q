using Api.Constants;

namespace Api.Middleware.Exceptions
{
    public class NotValidApiClientException : Exception
    {
        public NotValidApiClientException() : base(ApiMessages.NotValidApiClient)
        {
        }
    }
}
