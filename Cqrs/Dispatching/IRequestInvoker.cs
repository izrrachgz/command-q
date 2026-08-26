using Cqrs.Abstractions;

namespace Cqrs.Dispatching
{
    internal interface IRequestInvoker<TResponse>
    {
        Task<TResponse> Invoke(IRequest<TResponse> request, CancellationToken cancellationToken);
    }
}
