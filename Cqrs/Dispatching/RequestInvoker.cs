using Cqrs.Abstractions;

namespace Cqrs.Dispatching
{
    internal sealed class RequestInvoker<TRequest, TResponse> : IRequestInvoker<TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly RequestHandlerDelegate<TRequest, TResponse> _pipeline;

        public RequestInvoker(
            IRequestHandler<TRequest, TResponse> handler,
            IEnumerable<IPipelineBehavior<TRequest, TResponse>> behaviors)
        {
            RequestHandlerDelegate<TRequest, TResponse> pipeline = handler.Handle;

            foreach (IPipelineBehavior<TRequest, TResponse> behavior in behaviors.Reverse())
            {
                RequestHandlerDelegate<TRequest, TResponse> next = pipeline;
                pipeline = (request, cancellationToken) => behavior.Handle(request, next, cancellationToken);
            }

            _pipeline = pipeline;
        }

        public Task<TResponse> Invoke(IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            return _pipeline((TRequest)request, cancellationToken);
        }
    }
}
