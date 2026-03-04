using Public;
using Common.Cqrs;
using Domain.Entities;
using Domain.Interfaces;
using SqlKata.Execution;
using Common.Models.Response;
using Localization.Abstractions;
using Microsoft.Extensions.Logging;

namespace Application.AccountSessions.Commands
{
    public class UpdateAccountSessionCommandHandler : ICommandHandler<UpdateAccountSessionCommand, BasicResponse>
    {
        private readonly ITextProvider _messages;
        private readonly IWritableRepository _repository;
        private readonly ILogger<UpdateAccountSessionCommandHandler> _logger;

        private readonly string _message_account_updated;
        private readonly string _message_account_cannot_update;

        public UpdateAccountSessionCommandHandler(ITextProvider textProvider, IWritableRepository repository, ILogger<UpdateAccountSessionCommandHandler> logger)
        {
            _logger = logger;
            _repository = repository;
            _messages = textProvider;
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
            var account = await GetSession(request, cancellationToken);

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
                }
                else
                {
                    transaction.Rollback();
                    message = _message_account_cannot_update;
                }

                return new(saved, message);
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                _logger.LogError(ex, @"Error updating account for Id: {Id}", request.AccountId);

                return new(false, _message_account_cannot_update);
            }
            finally
            {
                _repository.Db.Connection.Close();
            }
        }

        /// <summary>
        /// Retrieves the active account session associated with the specified account identifier.
        /// </summary>
        /// <remarks>Only sessions that have not been marked as deleted are returned. If no active session
        /// exists for the specified account, the result will be null.</remarks>
        /// <param name="request">The command containing the account identifier for which to retrieve the session. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the account session if found;
        /// otherwise, null.</returns>
        private async Task<AccountSession?> GetSession(UpdateAccountSessionCommand request, CancellationToken cancellationToken)
        {
            return await _repository.Db.Query($@"{nameof(AccountSession)}")
                .Where($@"{nameof(AccountSession.AccountId)}", request.AccountId)
                .WhereNull($@"{nameof(AccountSession.Deleted)}")
                .Limit(1)
                .FirstOrDefaultAsync<AccountSession?>(cancellationToken: cancellationToken);
        }
    }
}
