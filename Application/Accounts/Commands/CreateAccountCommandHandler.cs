using Common.Hash;
using Common.Cqrs;
using Domain.Entities;
using Domain.Interfaces;
using SqlKata.Execution;
using Application.Constants;
using Common.Models.Response;
using Microsoft.Extensions.Logging;

namespace Application.Accounts.Commands
{
    public class CreateAccountCommandHandler : ICommandHandler<CreateAccountCommand, BasicResponse>
    {
        private readonly IWritableRepository _repository;
        private readonly ILogger<CreateAccountCommandHandler> _logger;

        public CreateAccountCommandHandler(IWritableRepository repository, ILogger<CreateAccountCommandHandler> logger)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task<BasicResponse> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            // Check if account with same email or username exists

            bool accountExists = await _repository.Db.Query($"{nameof(Account)}")
                .Where(
                    q => q.Where($@"{nameof(Account.Username)}", request.Username)
                    .OrWhere($@"{nameof(Account.Email)}", request.Email)
                )
                .ExistsAsync(cancellationToken: cancellationToken);

            if (accountExists)
            {
                return new(false, ApplicationMessages.CannotCreateAccount);
            }

            //Create payload objects for entities
            var now = DateTime.Now;

            var account = new
            {
                Username = request.Username,
                Email = request.Email,
                Password = Hash.ComputeSHA256(request.Password),
                Created = now,
                Modified = now
            };

            // Create transaction to ensure data integrity
            _repository.Db.Connection.Open();
            using var transaction = _repository.Db.Connection.BeginTransaction();

            try
            {
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
                    return new(false, ApplicationMessages.CannotCreateAccount);
                }

                var contact = new
                {
                    AccountId = insertedAccount.Id,
                    Name = $"Contact for {request.Username}",
                    EmailAddress = request.Email,
                    Created = now,
                    Modified = now
                };

                // Insert new contact record
                affected += await _repository.Db.Query($"{nameof(Contact)}")
                    .InsertAsync(contact, transaction, cancellationToken: cancellationToken);

                bool saved = affected >= 1;

                if (saved)
                {
                    transaction.Commit();
                }
                else
                {
                    transaction.Rollback();
                }

                return new(saved, saved ? ApplicationMessages.AccountSaved : ApplicationMessages.CannotCreateAccount);
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                _logger.LogError(ex, "Error creating account for Email: {Email}, Username: {Username}", request.Email, request.Username);

                return new(false, ApplicationMessages.CannotCreateAccount);
            }
        }
    }
}
