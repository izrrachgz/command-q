namespace Cqrs.Abstractions
{
    /// <summary>
    /// Dispatches an in-process request through its ordered pipeline to its single handler.
    /// </summary>
    public interface IRequestDispatcher
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    }
}
