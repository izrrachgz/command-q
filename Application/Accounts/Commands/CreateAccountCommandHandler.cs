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
    public class CreateAccountCommandHandler : ICommandHandler<CreateAccountCommand, BasicResponse>
    {
        private readonly ITextProvider _messages;
        private readonly IWritableRepository _repository;
        private readonly ILogger<CreateAccountCommandHandler> _logger;

        private readonly string _message_account_created;
        private readonly string _message_account_cannot_create;

        public CreateAccountCommandHandler(ITextProvider textProvider, IWritableRepository repository, ILogger<CreateAccountCommandHandler> logger)
        {
            _logger = logger;
            _repository = repository;
            _messages = textProvider;
            _message_account_created = _messages.Get(nameof(Messages.Account_Created));
            _message_account_cannot_create = _messages.Get(nameof(Messages.Account_Cannot_Create));
        }

        /// <summary>
        /// Handles the creation of a new user account based on the specified command.
        /// </summary>
        /// <remarks>If an account with the specified username or email already exists, the operation will
        /// not create a new account and will return a failure response. The method ensures data integrity by performing
        /// the operation within a transaction. If an error occurs during account creation, the transaction is rolled
        /// back and a failure response is returned.</remarks>
        /// <param name="request">The command containing the details required to create the account, including username, email, and password.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a BasicResponse indicating
        /// whether the account was created successfully and an associated message.</returns>
        public async Task<BasicResponse> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            bool accountExists = await AccountAlreadyExists(request, cancellationToken);

            if (accountExists)
                return new(false, _message_account_cannot_create);

            var now = DateTime.Now;

            // Create transaction to ensure data integrity
            _repository.Db.Connection.Open();
            using var transaction = _repository.Db.Connection.BeginTransaction();

            try
            {
                var account = new
                {
                    request.Username,
                    request.Email,
                    Password = Hash.ComputeSHA256(request.Password),
                    TokenVersion = 0,
                    Roles = new string[] { "User" },
                    FailedLoginAttempts = (byte)0,
                    Created = now,
                    Modified = now
                };

                // Insert new account record
                int affected = await _repository.Db.Query($"{nameof(Account)}")
                    .InsertAsync(account, transaction, cancellationToken: cancellationToken);

                Account insertedAccount = await _repository.Db.Query($"{nameof(Account)}")
                    .Where($@"{nameof(Account.Username)}", request.Username)
                    .Where($@"{nameof(Account.Email)}", request.Email)
                    .WhereNull($@"{nameof(Account.Deleted)}")
                    .OrderByDesc($@"{nameof(Account.Created)}")
                    .Limit(1)
                    .FirstOrDefaultAsync<Account>(transaction, cancellationToken: cancellationToken);

                if (insertedAccount == null || !Hash.CompareSHA256(account.Password, insertedAccount.Password))
                {
                    transaction.Rollback();

                    return new(false, _message_account_cannot_create);
                }

                var contact = new
                {
                    AccountId = insertedAccount.Id,
                    Name = $@"{request.Username}'s Contact.",
                    EmailAddress = request.Email,
                    Created = now,
                    Modified = now
                };

                // Insert new contact record
                affected += await _repository.Db.Query($"{nameof(Contact)}")
                    .InsertAsync(contact, transaction, cancellationToken: cancellationToken);

                var address = new
                {
                    AccountId = insertedAccount.Id,
                    Name = $@"{request.Username}'s Address.",
                    Line1 = string.Empty,
                    City = string.Empty,
                    PostalCode = string.Empty,
                    Country = string.Empty,
                    EmailAddress = request.Email,
                    Created = now,
                    Modified = now
                };

                affected += await _repository.Db.Query($"{nameof(Address)}")
                    .InsertAsync(address, transaction, cancellationToken: cancellationToken);

                bool saved = affected >= 1;

                string message = string.Empty;

                if (saved)
                {
                    transaction.Commit();
                    message = _message_account_created;
                }
                else
                {
                    transaction.Rollback();
                    message = _message_account_cannot_create;
                }

                return new(saved, message);
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                _logger.LogError(ex, "Error creating account for Email: {Email}, Username: {Username}", request.Email, request.Username);

                return new(false, _message_account_cannot_create);
            }
            finally
            {
                _repository.Db.Connection.Close();
            }
        }

        /// <summary>
        /// Determines whether an account with the specified username or email address already exists.
        /// </summary>
        /// <param name="request">The command containing the username and email address to check for existing accounts. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if an
        /// account with the specified username or email address exists; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> AccountAlreadyExists(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            return await _repository.Db.Query($"{nameof(Account)}")
               .Where(
                   q => q.Where($@"{nameof(Account.Username)}", request.Username)
                   .OrWhere($@"{nameof(Account.Email)}", request.Email)
               )
               .ExistsAsync(cancellationToken: cancellationToken);
        }
    }
}
