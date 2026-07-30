using Public;
using Common.Cqrs;
using Domain.Entities;
using SqlKata.Execution;
using Persistence.Interfaces;
using Common.Models.Response;
using Localization.Abstractions;
using Microsoft.Extensions.Logging;
using Application.Accounts.Services;

namespace Application.AccountSessions.Commands
{
    public class LockAccountSessionCommandHandler : ICommandHandler<LockAccountSessionCommand, BasicResponse>
    {
        private readonly ITextProvider _messages;
        private readonly AccountService _accountService;
        private readonly IWritableRepository _repository;
        private readonly ILogger<LockAccountSessionCommandHandler> _logger;

        private readonly string _message_account_locked;
        private readonly string _message_cannot_lock_account;

        public LockAccountSessionCommandHandler(AccountService accountService, ITextProvider textProvider, IWritableRepository repository, ILogger<LockAccountSessionCommandHandler> logger)
        {
            _logger = logger;
            _repository = repository;
            _messages = textProvider;
            _accountService = accountService;
            _message_account_locked = _messages.Get(nameof(Messages.Account_Locked));
            _message_cannot_lock_account = _messages.Get(nameof(Messages.Cannot_Lock_Account));
        }

        /// <summary>
        /// Locks the account for the specified duration. If the account is already locked, it will be updated with the new lock time.
        /// </summary>
        /// <param name="request">The command containing the details required to lock the account.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a BasicResponse indicating
        /// whether the account was locked successfully and an associated message.</returns>
        public async Task<BasicResponse> Handle(LockAccountSessionCommand request, CancellationToken cancellationToken)
        {
            var account = await _accountService.GetAccountSession(request.AccountId, cancellationToken);

            if (account == null)
            {
                _logger.LogWarning(@"Account session lock failed because account session was not found. AccountId: {AccountId}.", request.AccountId);
                return new(false, _message_cannot_lock_account);
            }

            var now = DateTime.Now;
            var utcNow = DateTime.UtcNow;

            account.LockedUntilUtc = utcNow.Add(request.LockTime);
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
                    message = _message_account_locked;
                    _logger.LogInformation(@"Account session locked. AccountId: {AccountId}. LockedUntilUtc: {LockedUntilUtc}.", request.AccountId, account.LockedUntilUtc);
                }
                else
                {
                    transaction.Rollback();
                    message = _message_cannot_lock_account;
                    _logger.LogWarning(@"Account session lock affected no records. AccountId: {AccountId}.", request.AccountId);
                }

                return new(saved, message);
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                _logger.LogError(ex, @"Error locking account for Id: {AccountId}", request.AccountId);

                return new(false, _message_cannot_lock_account);
            }
            finally
            {
                _repository.Db.Connection.Close();
            }
        }
    }
}
