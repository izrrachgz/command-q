namespace Cqrs.Abstractions
{
    /// <summary>
    /// Handles one command that returns a response.
    /// </summary>
    public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
    }

    /// <summary>
    /// Handles one command without a result payload.
    /// </summary>
    public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Unit>
        where TCommand : ICommand
    {
    }
}
