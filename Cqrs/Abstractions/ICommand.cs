namespace Cqrs.Abstractions
{
    /// <summary>
    /// Marks a request that changes application state and returns a response.
    /// </summary>
    public interface ICommand<out TResponse> : IRequest<TResponse>
    {
    }

    /// <summary>
    /// Marks a request that changes application state without a result payload.
    /// </summary>
    public interface ICommand : ICommand<Unit>
    {
    }
}
