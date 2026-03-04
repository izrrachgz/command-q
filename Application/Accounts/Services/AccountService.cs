using Domain.Entities;
using Domain.Interfaces;
using SqlKata.Execution;
using Localization.Abstractions;
using Microsoft.Extensions.Logging;

namespace Application.Accounts.Services
{
    public sealed class AccountService(ITextProvider textProvider, IWritableRepository repository, ILogger<AccountService> logger)
    {
        private readonly ITextProvider _messages = textProvider;
        private readonly IWritableRepository _repository = repository;
        private readonly ILogger<AccountService> _logger = logger;

        /// <summary>
        /// Gets the account details.
        /// </summary>
        /// <param name="accountId">The account id.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Account.</returns>
        public async Task<Account?> GetAccount(Guid accountId, CancellationToken cancellationToken = default)
        {
            return await _repository.Db.Query($@"{nameof(Account)}")
                .WhereNull($@"{nameof(Account.Deleted)}")
                .Where($@"{nameof(Account.Id)}", accountId)
                .Limit(1)
                .FirstOrDefaultAsync<Account?>(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Gets the account session details.
        /// </summary>
        /// <param name="accountId">The account id.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a AccountSession.</returns>
        public async Task<AccountSession?> GetAccountSession(Guid accountId, CancellationToken cancellationToken = default)
        {
            return await _repository.Db.Query($@"{nameof(AccountSession)}")
                .WhereNull($@"{nameof(AccountSession.Deleted)}")
                .Where($@"{nameof(AccountSession.AccountId)}", accountId)
                .Limit(1)
                .FirstOrDefaultAsync<AccountSession?>(cancellationToken: cancellationToken);
        }
    }
}
