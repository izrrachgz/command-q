using Application.Constants;

namespace Search.Middleware.Exceptions
{
    public class NotValidApiKeysException : Exception
    {
        public NotValidApiKeysException() : base(ApplicationMessages.NotValidApiKeys)
        {
        }
    }
}
