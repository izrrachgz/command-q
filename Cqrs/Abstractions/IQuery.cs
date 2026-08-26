namespace Cqrs.Abstractions
{
    /// <summary>
    /// Marks a read-only request that returns a response.
    /// </summary>
    public interface IQuery<out TResponse> : IRequest<TResponse>
    {
    }
}
