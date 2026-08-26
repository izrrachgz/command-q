namespace Cqrs.Exceptions
{
    /// <summary>
    /// Thrown when more than one handler registration is attempted for a request type.
    /// </summary>
    public sealed class DuplicateRequestHandlerException(Type requestType)
        : InvalidOperationException($"A handler is already registered for request type '{requestType.FullName}'.")
    {
        public Type RequestType { get; } = requestType;
    }
}
