namespace Cqrs.Abstractions
{
    /// <summary>
    /// Wraps request execution with a cross-cutting concern.
    /// </summary>
    public interface IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TRequest, TResponse> next,
            CancellationToken cancellationToken);
    }
}
