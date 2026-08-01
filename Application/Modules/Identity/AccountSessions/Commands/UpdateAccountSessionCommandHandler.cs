using Public;
using Common.Cqrs;
using SqlKata.Execution;
using Common.Models.Response;
using Persistence.Interfaces;
using Localization.Abstractions;
using Microsoft.Extensions.Logging;
using Application.Modules.Identity.Logging;
using Application.Modules.Identity.Accounts.Services;

namespace Application.Modules.Identity.AccountSessions.Commands
{
    public class UpdateAccountSessionCommandHandler : ICommandHandler<UpdateAccountSessionCommand, BasicResponse>
    {
        private readonly ITextProvider _messages;
        private readonly AccountService _accountService;
        private readonly IWritableRepository _repository;
        private readonly ILogger<UpdateAccountSessionCommandHandler> _logger;

        private readonly string _message_account_updated;
        private readonly string _message_account_cannot_update;

        public UpdateAccountSessionCommandHandler(AccountService accountService, ITextProvider textProvider, IWritableRepository repository, ILogger<UpdateAccountSessionCommandHandler> logger)
        {
            _logger = logger;
            _repository = repository;
            _messages = textProvider;
            _accountService = accountService;
            _message_account_updated = _messages.Get(nameof(Messages.Updated), nameof(Account));
            _message_account_cannot_update = _messages.Get(nameof(Messages.Cannot_Update), nameof(Account));
        }

        /// <summary>
        /// Handles an update to an account by applying the specified changes and persisting them to the data store.
        /// </summary>
        /// <remarks>If the specified account does not exist, the operation returns a failure response.
        /// The update is performed within a transaction to ensure data integrity. If an error occurs during the update,
        /// the transaction is rolled back and a failure response is returned.</remarks>
        /// <param name="request">The command containing the updated account information to apply. Must not be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a BasicResponse indicating
        /// whether the update was successful and includes a corresponding message.</returns>
        public async Task<BasicResponse> Handle(UpdateAccountSessionCommand request, CancellationToken cancellationToken)
        {
            var account = await _accountService.GetAccountSession(request.AccountId, cancellationToken);

            bool create = account is null;

            var now = DateTime.Now;

            #region Patch values

            account ??= new AccountSession
            {
                AccountId = request.AccountId,
                Created = now,
            };

            if (request.TokenVersion.HasValue)
                account.TokenVersion = request.TokenVersion.Value;

            if (request.FailedLoginAttempts.HasValue)
                account.FailedLoginAttempts = request.FailedLoginAttempts.Value;

            if (request.LockedUntilUtc.HasValue)
                account.LockedUntilUtc = request.LockedUntilUtc;

            account.Modified = now;

            #endregion

            // Create transaction to ensure data integrity
            _repository.Db.Connection.Open();
            using var transaction = _repository.Db.Connection.BeginTransaction();

            try
            {
                //Isert or Update account session record

                int affected = 0;
                if (create)
                {
                    affected = await _repository.Db.Query($@"{nameof(AccountSession)}")
                   .InsertAsync(account, transaction, cancellationToken: cancellationToken);
                }
                else
                {
                    affected = await _repository.Db.Query($@"{nameof(AccountSession)}")
                   .UpdateAsync(account, transaction, cancellationToken: cancellationToken);
                }

                bool saved = affected >= 1;

                string message = string.Empty;

                if (saved)
                {
                    transaction.Commit();
                    message = _message_account_updated;
                    _logger.AccountSessionChanged(create ? "created" : "updated", request.AccountId, account.TokenVersion);
                }
                else
                {
                    transaction.Rollback();
                    message = _message_account_cannot_update;
                    _logger.AccountSessionAffectedNoRecords("update", request.AccountId);
                }

                return new(saved, message);
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                _logger.AccountSessionChangeFailed(ex, "updating", request.AccountId);

                return new(false, _message_account_cannot_update);
            }
            finally
            {
                _repository.Db.Connection.Close();
            }
        }
    }
}
