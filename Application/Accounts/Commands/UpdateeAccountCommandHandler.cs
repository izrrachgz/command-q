using Public;
using Common.Cqrs;
using Common.Hash;
using Domain.Entities;
using Domain.Interfaces;
using SqlKata.Execution;
using Common.Models.Response;
using Localization.Abstractions;
using Microsoft.Extensions.Logging;

namespace Application.Accounts.Commands
{
    public class UpdateAccountCommandHandler : ICommandHandler<UpdateAccountCommand, BasicResponse>
    {
        private readonly ITextProvider _messages;
        private readonly IWritableRepository _repository;
        private readonly ILogger<UpdateAccountCommandHandler> _logger;

        private readonly string _message_account_updated;
        private readonly string _message_account_cannot_update;

        public UpdateAccountCommandHandler(ITextProvider textProvider, IWritableRepository repository, ILogger<UpdateAccountCommandHandler> logger)
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
        public async Task<BasicResponse> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
        {
            var account = await GetAccount(request, cancellationToken);

            if (account == null)
                return new(false, _message_account_cannot_update);

            var now = DateTime.Now;

            #region Patch values

            if (!string.IsNullOrEmpty(request.Password))
                account.Password = Hash.ComputeSHA256(request.Password);

            //TODO: Decide if we want to allow username updates
            //if (!string.IsNullOrEmpty(request.Username) && account.Username != request.Username)
            //    account.Username = request.Username;

            if (!string.IsNullOrEmpty(request.Email) && account.Email != request.Email)
                account.Email = request.Email;

            if (request.TokenVersion.HasValue)
                account.TokenVersion = request.TokenVersion.Value;

            if (account.Roles != null && request.Roles != null && request.Roles.Length > 0)
                account.Roles = string.Join(",", request.Roles);

            if (request.FailedLoginAttempts.HasValue)
                account.FailedLoginAttempts = request.FailedLoginAttempts.Value;

            if (request.LockedUntilUtc.HasValue)
                account.LockedUntilUtc = request.LockedUntilUtc;

            #endregion

            account.Modified = now;

            // Create transaction to ensure data integrity
            _repository.Db.Connection.Open();
            using var transaction = _repository.Db.Connection.BeginTransaction();

            try
            {
                //Update account record
                int affected = await _repository.Db.Query($"{nameof(Account)}")
                    .UpdateAsync(account, transaction, cancellationToken: cancellationToken);

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

                _logger.LogError(ex, "Error updating account for Email: {Email}, Username: {Username}", request.Email, request.Username);

                return new(false, _message_account_cannot_update);
            }
            finally
            {
                _repository.Db.Connection.Close();
            }
        }

        /// <summary>
        /// Retrieves the account associated with the specified update command, if it exists and has not been deleted.
        /// </summary>
        /// <param name="request">The update command containing the identifier of the account to retrieve. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the account if found and not
        /// deleted; otherwise, null.</returns>
        private async Task<Account> GetAccount(UpdateAccountCommand request, CancellationToken cancellationToken)
        {
            return await _repository.Db.Query($"{nameof(Account)}")
                .Where($@"{nameof(Account.Id)}", request.Id)
                .WhereNull($@"{nameof(Account.Deleted)}")
                .Limit(1)
                .FirstOrDefaultAsync<Account>(cancellationToken: cancellationToken);
        }
    }
}
