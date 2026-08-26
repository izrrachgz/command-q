namespace Cqrs.Abstractions
{
    /// <summary>
    /// Handles one query request.
    /// </summary>
    public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
    }
}
