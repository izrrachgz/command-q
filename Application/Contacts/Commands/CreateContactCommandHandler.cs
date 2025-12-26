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

        private readonly string _message_contact_created;
        private readonly string _message_contact_cannot_create;

        public CreateContactCommandHandler(ITextProvider textProvider, IWritableRepository repository, ILogger<CreateContactCommandHandler> logger)
        {
            _logger = logger;
            _repository = repository;
            _messages = textProvider;

            _message_contact_created = _messages.Get(nameof(Messages.Created), nameof(Contact));
            _message_contact_cannot_create = _messages.Get(nameof(Messages.Cannot_Create), nameof(Contact));
        }

        /// <summary>
        /// Handles the creation of a new contact based on the specified command.
        /// </summary>
        /// <remarks>The operation is performed within a database transaction to ensure data integrity. If
        /// the contact cannot be created, the transaction is rolled back and an error message is returned. An error is
        /// logged if an exception occurs during the process.</remarks>
        /// <param name="request">The command containing the details of the contact to create. Must not be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a BasicResponse indicating
        /// whether the contact was created successfully and an associated message.</returns>
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
                    message = _message_contact_created;
                }
                else
                {
                    transaction.Rollback();
                    message = _message_contact_cannot_create;
                }

                return new(saved, message);
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                _logger.LogError(ex, "Error creating contact for AccountId: {AccountId}", request.AccountId);

                return new(false, _message_contact_cannot_create);
            }
            finally
            {
                _repository.Db.Connection.Close();
            }
        }
    }
}
