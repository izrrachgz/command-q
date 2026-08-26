namespace Cqrs.Abstractions
{
    /// <summary>
    /// Handles one concrete request type.
    /// </summary>
    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
