using Search.Constants;

namespace Search.Middleware.Exceptions
{
    public class NotValidApiKeysException : Exception
    {
        public NotValidApiKeysException() : base(SearchMessages.NotValidApiKeys)
        {
        }
    }
}
