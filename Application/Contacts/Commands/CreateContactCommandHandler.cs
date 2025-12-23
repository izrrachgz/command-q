using Common.Cqrs;
using Domain.Entities;
using Domain.Interfaces;
using SqlKata.Execution;
using Application.Constants;
using Common.Models.Response;
using Microsoft.Extensions.Logging;

namespace Application.Contacts.Commands
{
    public class CreateContactCommandHandler : ICommandHandler<CreateContactCommand, BasicResponse>
    {
        private readonly IWritableRepository _repository;
        private readonly ILogger<CreateContactCommandHandler> _logger;

        public CreateContactCommandHandler(IWritableRepository repository, ILogger<CreateContactCommandHandler> logger)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task<BasicResponse> Handle(CreateContactCommand request, CancellationToken cancellationToken)
        {
            var now = DateTime.Now;

            // Create transaction to ensure data integrity
            _repository.Db.Connection.Open();
            using var transaction = _repository.Db.Connection.BeginTransaction();

            try
            {
                var contact = new
                {
                    request.AccountId,
                    request.Name,
                    request.EmailAddress,
                    request.Description,
                    request.PhoneNumber,
                    Modified = now
                };

                // Insert new contact record
                int affected = await _repository.Db.Query($"{nameof(Contact)}")
                       .InsertAsync(contact, transaction, cancellationToken: cancellationToken);

                bool saved = affected >= 1;

                if (saved)
                    transaction.Commit();
                else
                    transaction.Rollback();

                return new(saved, saved ? ApplicationMessages.AccountSaved : ApplicationMessages.CannotCreateAccount);
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                _logger.LogError(ex, "Error creating contact for AccountId: {AccountId}", request.AccountId);

                return new(false, @"Cannot create contact.");
            }
            finally
            {
                _repository.Db.Connection.Close();
            }
        }
    }
}
