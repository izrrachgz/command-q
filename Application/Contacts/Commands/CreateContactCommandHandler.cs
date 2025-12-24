using Public;
using Common.Cqrs;
using Domain.Entities;
using Domain.Interfaces;
using SqlKata.Execution;
using Common.Models.Response;
using Localization.Abstractions;
using Microsoft.Extensions.Logging;

namespace Application.Contacts.Commands
{
    public class CreateContactCommandHandler : ICommandHandler<CreateContactCommand, BasicResponse>
    {
        private readonly ITextProvider _messages;
        private readonly IWritableRepository _repository;
        private readonly ILogger<CreateContactCommandHandler> _logger;

        private readonly string _contact_created;
        private readonly string _contact_cannot_create;

        public CreateContactCommandHandler(ITextProvider textProvider, IWritableRepository repository, ILogger<CreateContactCommandHandler> logger)
        {
            _logger = logger;
            _repository = repository;
            _messages = textProvider;

            _contact_created = _messages.Get(nameof(Messages.Contact_Created));
            _contact_cannot_create = _messages.Get(nameof(Messages.Contact_Cannot_Create));
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

                string message = string.Empty;

                if (saved)
                {
                    transaction.Commit();
                    message = _contact_created;
                }
                else
                {
                    transaction.Rollback();
                    message = _contact_cannot_create;
                }

                return new(saved, message);
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                _logger.LogError(ex, "Error creating contact for AccountId: {AccountId}", request.AccountId);

                return new(false, _contact_cannot_create);
            }
            finally
            {
                _repository.Db.Connection.Close();
            }
        }
    }
}
