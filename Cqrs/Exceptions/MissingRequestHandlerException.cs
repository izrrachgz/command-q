namespace Cqrs.Exceptions
{
    /// <summary>
    /// Thrown when a request is dispatched without an explicit handler registration.
    /// </summary>
    public sealed class MissingRequestHandlerException(Type requestType)
        : InvalidOperationException($"No handler is registered for request type '{requestType.FullName}'.")
    {
        public Type RequestType { get; } = requestType;
    }
}
