using Persistence.Interfaces;
using Localization.Abstractions;
using Microsoft.Extensions.Logging;

namespace Application.Modules.Identity.Accounts.Services
{
    public sealed class AccountService(ITextProvider textProvider, IReadOnlyRepository repository, ILogger<AccountService> logger)
    {
        private readonly ITextProvider _messages = textProvider;
        private readonly IReadOnlyRepository _repository = repository;
        private readonly ILogger<AccountService> _logger = logger;

        /// <summary>
        /// Gets the account details.
        /// </summary>
        /// <param name="accountId">The account id.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Account.</returns>
        public async Task<Account?> GetAccount(Guid accountId, CancellationToken cancellationToken = default)
        {
            var query = _repository.Query($@"{nameof(Account)}")
                .WhereNull($@"{nameof(Account.Deleted)}")
                .Where($@"{nameof(Account.Id)}", accountId)
                .Limit(1);

            return await _repository.FirstOrDefaultAsync<Account?>(query, cancellationToken);
        }
    }
}
