using Cqrs.Abstractions;
using Application.Abstractions;
using Persistence.Abstractions;

namespace Application.RowLevelSecurity
{
    /// <summary>
    /// Establishes a fail-closed database RLS context around every application request.
    /// </summary>
    internal sealed class AccountRowLevelSecurityBehavior<TRequest, TResponse>(
        AccountRowLevelSecurityContext context,
        IReadOnlyRepository readOnlyRepository,
        IWritableRepository writableRepository) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TRequest, TResponse> next,
            CancellationToken cancellationToken)
        {
            IReadOnlyRepository repository = request switch
            {
                IQuery<TResponse> => readOnlyRepository,
                ICommand<TResponse> => writableRepository,
                _ => throw new InvalidOperationException(
                    $"Request '{typeof(TRequest).Name}' must implement either IQuery<TResponse> or ICommand<TResponse>.")
            };

            if (repository is not IDatabaseSessionContextRepository sessionContextRepository)
            {
                throw new InvalidOperationException(
                    $"Repository '{repository.GetType().Name}' does not support database session context.");
            }

            IAsyncDisposable scope = request switch
            {
                IAccountScopedRequest and ISystemScopedRequest => throw new InvalidOperationException(
                    $"Request '{typeof(TRequest).Name}' cannot be both account-scoped and system-scoped."),
                IAccountScopedRequest accountRequest => await context.BeginAccountScopeAsync(
                    sessionContextRepository,
                    accountRequest.AccountId,
                    cancellationToken),
                ISystemScopedRequest => await context.BeginSystemScopeAsync(sessionContextRepository, cancellationToken),
                _ => await context.BeginRestrictiveScopeAsync(sessionContextRepository, cancellationToken)
            };

            await using (scope)
            {
                return await next(request, cancellationToken);
            }
        }
    }
}
