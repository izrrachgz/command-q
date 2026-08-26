using Public;
using Time.Extensions;
using Common.Cqrs;
using System.Data;
using SqlKata.Execution;
using Common.Models.Response;
using Persistence.Abstractions;
using Localization.Abstractions;
using Microsoft.Extensions.Logging;
using Application.Modules.Identity.Logging;

namespace Application.Modules.Identity.Accounts.Commands
{
    internal sealed class CreateAccountCommandHandler(
        ITextProvider textProvider,
        IWritableRepository repository,
        TimeProvider timeProvider,
        ILogger<CreateAccountCommandHandler> logger)
        : ICommandHandler<CreateAccountCommand, BasicResponse>
    {
        private readonly string _messageAccountCreated = textProvider.Get(nameof(Messages.Created), nameof(Account));
        private readonly string _messageAccountCannotCreate = textProvider.Get(nameof(Messages.Cannot_Create), nameof(Account));
        private readonly string _messageDefaultContactName = textProvider.Get(nameof(Messages.Default_Contact_Name));
        private readonly string _messageDefaultAddressName = textProvider.Get(nameof(Messages.Default_Address_Name));

        public async Task<BasicResponse> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            var now = timeProvider.GetCurrentUtcDateTime();
            var accountId = Guid.NewGuid();

            repository.Db.Connection.Open();
            using var transaction = repository.Db.Connection.BeginTransaction();

            try
            {
                if (await IdentityAlreadyExists(request, transaction, cancellationToken))
                {
                    transaction.Commit();
                    logger.AccountProvisioningAlreadyLinked(request.Username);
                    return new(true, _messageAccountCreated);
                }

                var existingAccount = await GetExistingAccountByUsernameAndEmail(request, transaction, cancellationToken);

                if (existingAccount is not null)
                {
                    if (await AccountHasAnyIdentity(existingAccount.Id, transaction, cancellationToken))
                    {
                        transaction.Rollback();
                        logger.AccountCreationDuplicate(request.Username);
                        return new(false, _messageAccountCannotCreate);
                    }

                    var linked = await LinkIdentity(existingAccount.Id, request, now, transaction, cancellationToken);

                    if (linked <= 0)
                    {
                        transaction.Rollback();
                        logger.AccountCreationVerificationFailed(request.Username);
                        return new(false, _messageAccountCannotCreate);
                    }

                    transaction.Commit();
                    logger.AccountIdentityLinked(existingAccount.Id, request.Username);
                    return new(true, _messageAccountCreated);
                }

                if (await UsernameOrEmailAlreadyExists(request, transaction, cancellationToken))
                {
                    transaction.Rollback();
                    logger.AccountCreationDuplicate(request.Username);
                    return new(false, _messageAccountCannotCreate);
                }

                var account = new
                {
                    Id = accountId,
                    Username = request.Username.Trim(),
                    Email = request.Email.Trim(),
                    CreatedUtc = now,
                    ModifiedUtc = now
                };

                var affected = await repository.Db.Query(nameof(Account))
                    .InsertAsync(account, transaction, cancellationToken: cancellationToken);

                affected += await repository.Db.Query(nameof(AccountIdentity))
                    .InsertAsync(new
                    {
                        AccountId = accountId,
                        Issuer = request.IdentityIssuer,
                        Subject = request.IdentitySubject,
                        CreatedUtc = now,
                        ModifiedUtc = now
                    }, transaction, cancellationToken: cancellationToken);

                affected += await repository.Db.Query(nameof(Contact))
                    .InsertAsync(new
                    {
                        AccountId = accountId,
                        Name = string.Format(_messageDefaultContactName, request.Username),
                        EmailAddress = request.Email.Trim(),
                        CreatedUtc = now,
                        ModifiedUtc = now
                    }, transaction, cancellationToken: cancellationToken);

                affected += await repository.Db.Query(nameof(Address))
                    .InsertAsync(new
                    {
                        AccountId = accountId,
                        Name = string.Format(_messageDefaultAddressName, request.Username),
                        Line1 = string.Empty,
                        City = string.Empty,
                        PostalCode = string.Empty,
                        Country = string.Empty,
                        EmailAddress = request.Email.Trim(),
                        CreatedUtc = now,
                        ModifiedUtc = now
                    }, transaction, cancellationToken: cancellationToken);

                var insertedAccount = await repository.Db.Query(nameof(Account))
                    .Where(nameof(Account.Id), accountId)
                    .WhereNull(nameof(Account.DeletedUtc))
                    .FirstOrDefaultAsync<Account>(transaction, cancellationToken: cancellationToken);

                if (insertedAccount is null || affected < 4)
                {
                    transaction.Rollback();
                    logger.AccountCreationVerificationFailed(request.Username);
                    return new(false, _messageAccountCannotCreate);
                }

                transaction.Commit();
                logger.AccountCreated(accountId, request.Username);

                return new(true, _messageAccountCreated);
            }
            catch (Exception exception)
            {
                transaction.Rollback();
                logger.AccountCreationFailed(exception, request.Email, request.Username);

                return new(false, _messageAccountCannotCreate);
            }
            finally
            {
                repository.Db.Connection.Close();
            }
        }

        private async Task<bool> IdentityAlreadyExists(CreateAccountCommand request, IDbTransaction transaction, CancellationToken cancellationToken)
        {
            return await repository.Db.Query(nameof(AccountIdentity))
                .WhereNull(nameof(AccountIdentity.DeletedUtc))
                .Where(nameof(AccountIdentity.Issuer), request.IdentityIssuer)
                .Where(nameof(AccountIdentity.Subject), request.IdentitySubject)
                .ExistsAsync(transaction, cancellationToken: cancellationToken);
        }

        private async Task<Account?> GetExistingAccountByUsernameAndEmail(CreateAccountCommand request, IDbTransaction transaction, CancellationToken cancellationToken)
        {
            return await repository.Db.Query(nameof(Account))
                .WhereNull(nameof(Account.DeletedUtc))
                .Where(nameof(Account.Username), request.Username.Trim())
                .Where(nameof(Account.Email), request.Email.Trim())
                .FirstOrDefaultAsync<Account>(transaction, cancellationToken: cancellationToken);
        }

        private async Task<bool> UsernameOrEmailAlreadyExists(CreateAccountCommand request, IDbTransaction transaction, CancellationToken cancellationToken)
        {
            return await repository.Db.Query(nameof(Account))
                .WhereNull(nameof(Account.DeletedUtc))
                .Where(query => query
                    .Where(nameof(Account.Username), request.Username.Trim())
                    .OrWhere(nameof(Account.Email), request.Email.Trim()))
                .ExistsAsync(transaction, cancellationToken: cancellationToken);
        }

        private async Task<bool> AccountHasAnyIdentity(Guid accountId, IDbTransaction transaction, CancellationToken cancellationToken)
        {
            return await repository.Db.Query(nameof(AccountIdentity))
                .WhereNull(nameof(AccountIdentity.DeletedUtc))
                .Where(nameof(AccountIdentity.AccountId), accountId)
                .ExistsAsync(transaction, cancellationToken: cancellationToken);
        }

        private async Task<int> LinkIdentity(Guid accountId, CreateAccountCommand request, DateTime now, IDbTransaction transaction, CancellationToken cancellationToken)
        {
            return await repository.Db.Query(nameof(AccountIdentity))
                .InsertAsync(new
                {
                    AccountId = accountId,
                    Issuer = request.IdentityIssuer,
                    Subject = request.IdentitySubject,
                    CreatedUtc = now,
                    ModifiedUtc = now
                }, transaction, cancellationToken: cancellationToken);
        }
    }
}
