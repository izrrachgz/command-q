using Public;
using Common.Cqrs;
using Domain.Entities;
using Domain.Interfaces;
using SqlKata.Execution;
using Common.Models.Response;
using Localization.Abstractions;
using Microsoft.Extensions.Logging;

namespace Application.Addresses.Commands
{
    public class CreateAddressCommandHandler : ICommandHandler<CreateAddressCommand, BasicResponse>
    {
        private readonly ITextProvider _messages;
        private readonly IWritableRepository _repository;
        private readonly ILogger<CreateAddressCommandHandler> _logger;

        private readonly string _message_address_created;
        private readonly string _message_address_cannot_create;

        public CreateAddressCommandHandler(ITextProvider textProvider, IWritableRepository repository, ILogger<CreateAddressCommandHandler> logger)
        {
            _logger = logger;
            _repository = repository;
            _messages = textProvider;
            _message_address_created = _messages.Get(nameof(Messages.Created), nameof(Address));
            _message_address_cannot_create = _messages.Get(nameof(Messages.Cannot_Create), nameof(Address));
        }

        /// <summary>
        /// Handles the creation of a new address for the specified account.
        /// </summary>
        /// <remarks>The operation is performed within a database transaction to ensure data integrity. If
        /// the address cannot be created, the transaction is rolled back and an error message is returned.</remarks>
        /// <param name="request">The command containing the details of the address to create, including account information and address
        /// fields.</param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a BasicResponse indicating
        /// whether the address was created successfully and an associated message.</returns>
        public async Task<BasicResponse> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
        {
            var now = DateTime.Now;

            // Create transaction to ensure data integrity
            _repository.Db.Connection.Open();
            using var transaction = _repository.Db.Connection.BeginTransaction();

            try
            {
                var address = new
                {
                    request.AccountId,
                    request.Name,
                    request.Line1,
                    request.City,
                    request.PostalCode,
                    request.Country,
                    request.EmailAddress,
                    Modified = now
                };

                int affected = await _repository.Db.Query($"{nameof(Address)}")
                    .InsertAsync(address, transaction, cancellationToken: cancellationToken);

                bool saved = affected >= 1;

                string message = string.Empty;

                if (saved)
                {
                    transaction.Commit();
                    message = _message_address_created;
                }
                else
                {
                    transaction.Rollback();
                    message = _message_address_cannot_create;
                }

                return new(saved, message);
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                _logger.LogError(ex, "Error creating address for AccountId: {AccountId}", request.AccountId);

                return new(false, _message_address_cannot_create);
            }
            finally
            {
                _repository.Db.Connection.Close();
            }
        }
    }
}
