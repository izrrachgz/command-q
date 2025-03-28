using MediatR;

namespace Common.Cqrs
{
    public interface IQuery<out T> : IRequest<T>
    {
    }
}
