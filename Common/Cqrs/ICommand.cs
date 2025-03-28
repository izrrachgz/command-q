using MediatR;

namespace Common.Cqrs
{
    public interface ICommand<out T> : IRequest<T>
    {
    }

    public interface ICommand : ICommand<Unit>, IRequest
    {
    }
}
