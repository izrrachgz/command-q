using Cqrs.Abstractions;
using Cqrs.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Cqrs.Dispatching
{
    internal sealed class RequestDispatcher(IServiceProvider serviceProvider) : IRequestDispatcher
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            Type requestType = request.GetType();
            var invoker = serviceProvider.GetKeyedService<IRequestInvoker<TResponse>>(requestType)
                ?? throw new MissingRequestHandlerException(requestType);

            return invoker.Invoke(request, cancellationToken);
        }
    }
}
