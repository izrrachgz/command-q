using Persistence.Abstractions;

namespace Application.RowLevelSecurity
{
    /// <summary>
    /// Owns the nested row-level-security state for one dependency-injection scope.
    /// </summary>
    internal sealed class AccountRowLevelSecurityContext
    {
        private const string AccountIdKey = "command_q.account_id";
        private const string BypassKey = "command_q.rls_bypass";

        private readonly Dictionary<IDatabaseSessionContextRepository, IReadOnlyCollection<DatabaseSessionValue>> _currentValues =
            new(ReferenceEqualityComparer.Instance);

        public Task<IAsyncDisposable> BeginAccountScopeAsync(
            IDatabaseSessionContextRepository repository,
            Guid accountId,
            CancellationToken cancellationToken)
        {
            if (accountId == Guid.Empty)
                throw new InvalidOperationException("An account-scoped request must provide a non-empty account identifier.");

            DatabaseSessionValue[] values =
            [
                new(BypassKey, false),
                new(AccountIdKey, accountId)
            ];

            return BeginScopeAsync(repository, values, cancellationToken);
        }

        public Task<IAsyncDisposable> BeginSystemScopeAsync(
            IDatabaseSessionContextRepository repository,
            CancellationToken cancellationToken)
        {
            DatabaseSessionValue[] values =
            [
                new(AccountIdKey, null),
                new(BypassKey, true)
            ];

            return BeginScopeAsync(repository, values, cancellationToken);
        }

        public Task<IAsyncDisposable> BeginRestrictiveScopeAsync(
            IDatabaseSessionContextRepository repository,
            CancellationToken cancellationToken)
        {
            return BeginScopeAsync(repository, RestrictiveValues(), cancellationToken);
        }

        private async Task<IAsyncDisposable> BeginScopeAsync(
            IDatabaseSessionContextRepository repository,
            IReadOnlyCollection<DatabaseSessionValue> values,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<DatabaseSessionValue> previousValues = _currentValues.TryGetValue(repository, out var currentValues)
                ? currentValues
                : RestrictiveValues();

            await repository.SetSessionContextAsync(values, cancellationToken);
            _currentValues[repository] = values;

            return new Scope(this, repository, previousValues);
        }

        private async ValueTask RestoreAsync(
            IDatabaseSessionContextRepository repository,
            IReadOnlyCollection<DatabaseSessionValue> values)
        {
            try
            {
                await repository.SetSessionContextAsync(values);
            }
            finally
            {
                _currentValues[repository] = values;
            }
        }

        private static DatabaseSessionValue[] RestrictiveValues()
        {
            return
            [
                new(BypassKey, false),
                new(AccountIdKey, null)
            ];
        }

        private sealed class Scope(
            AccountRowLevelSecurityContext context,
            IDatabaseSessionContextRepository repository,
            IReadOnlyCollection<DatabaseSessionValue> previousValues) : IAsyncDisposable
        {
            private bool _disposed;

            public async ValueTask DisposeAsync()
            {
                if (_disposed)
                    return;

                _disposed = true;
                await context.RestoreAsync(repository, previousValues);
            }
        }
    }
}
