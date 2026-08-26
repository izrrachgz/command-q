namespace Cqrs.Abstractions
{
    /// <summary>
    /// Represents the next stage in a strongly typed request pipeline.
    /// </summary>
    public delegate Task<TResponse> RequestHandlerDelegate<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>;
}
