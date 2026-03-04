using Public;
using Common.Cqrs;
using Domain.Entities;
using Domain.Interfaces;
using SqlKata.Execution;
using Common.Models.Response;
using Localization.Abstractions;
using Microsoft.Extensions.Logging;
using Application.Accounts.Services;

namespace Application.AccountSessions.Commands
{
    public class UnlockAccountSessionCommandHandler : ICommandHandler<UnlockAccountSessionCommand, BasicResponse>
    {
        private readonly ITextProvider _messages;
        private readonly AccountService _accountService;
        private readonly IWritableRepository _repository;
        private readonly ILogger<UnlockAccountSessionCommandHandler> _logger;

        private readonly string _message_account_unlocked;
        private readonly string _message_cannot_unlock_account;

        public UnlockAccountSessionCommandHandler(AccountService accountService, ITextProvider textProvider, IWritableRepository repository, ILogger<UnlockAccountSessionCommandHandler> logger)
        {
            _logger = logger;
            _repository = repository;
            _messages = textProvider;
            _accountService = accountService;
            _message_account_unlocked = _messages.Get(nameof(Messages.Account_Unlocked));
            _message_cannot_unlock_account = _messages.Get(nameof(Messages.Cannot_Unlock_Account));
        }

        /// <summary>
        /// Unlocks the account.
        /// </summary>
        /// <param name="request">The command containing the details required to unlock the account.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a BasicResponse indicating
        /// whether the account was unlocked successfully and an associated message.</returns>
        public async Task<BasicResponse> Handle(UnlockAccountSessionCommand request, CancellationToken cancellationToken)
        {
            var account = await _accountService.GetAccountSession(request.AccountId, cancellationToken);

            if (account == null)
                return new(false, _message_cannot_unlock_account);

            var now = DateTime.Now;

            account.LockedUntilUtc = null;
            account.Modified = now;

            // Create transaction to ensure data integrity
            _repository.Db.Connection.Open();
            using var transaction = _repository.Db.Connection.BeginTransaction();

            try
            {
                // Update lock record
                int affected = await _repository.Db.Query($@"{nameof(AccountSession)}")
                    .UpdateAsync(account, transaction, cancellationToken: cancellationToken);

                bool saved = affected >= 1;

                string message = string.Empty;

                if (saved)
                {
                    transaction.Commit();
                    message = _message_account_unlocked;
                }
                else
                {
                    transaction.Rollback();
                    message = _message_cannot_unlock_account;
                }

                return new(saved, message);
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                _logger.LogError(ex, @"Error unlocking account for Id: {AccountId}", request.AccountId);

                return new(false, _message_cannot_unlock_account);
            }
            finally
            {
                _repository.Db.Connection.Close();
            }
        }
    }
}
