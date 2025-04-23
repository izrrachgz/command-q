using Api.Constants;

namespace Api.Middleware.Exceptions
{
    public class NotValidApiKeysException : Exception
    {
        public NotValidApiKeysException() : base(ApiMessages.NotValidApiKeys)
        {
        }
    }
}
