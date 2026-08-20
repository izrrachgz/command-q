using Persistence.Abstractions;

namespace Application.Modules.Identity.Accounts.Preferences.Services
{
    /// <summary>Provides read access to active account preferences.</summary>
    internal sealed class AccountPreferenceService(IReadOnlyRepository repository)
    {
        public async Task<AccountPreference?> Get(Guid accountId, CancellationToken cancellationToken = default)
        {
            var query = repository.Query(nameof(AccountPreference))
                .WhereNull(nameof(AccountPreference.DeletedUtc))
                .Where(nameof(AccountPreference.AccountId), accountId)
                .Limit(1);

            return await repository.FirstOrDefaultAsync<AccountPreference?>(query, cancellationToken);
        }
    }
}
