namespace Cqrs.Abstractions
{
    /// <summary>
    /// Marks an in-process request that produces one response.
    /// </summary>
    /// <typeparam name="TResponse">Response produced by the request handler.</typeparam>
    public interface IRequest<out TResponse>
    {
    }
}
