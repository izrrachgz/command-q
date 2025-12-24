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

        private readonly string _address_created;
        private readonly string _address_cannot_create;

        public CreateAddressCommandHandler(ITextProvider textProvider, IWritableRepository repository, ILogger<CreateAddressCommandHandler> logger)
        {
            _logger = logger;
            _repository = repository;
            _messages = textProvider;
            _address_created = _messages.Get(nameof(Messages.Address_Created));
            _address_cannot_create = _messages.Get(nameof(Messages.Address_Cannot_Create));
        }

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
                    message = _address_created;
                }
                else
                {
                    transaction.Rollback();
                    message = _address_cannot_create;
                }

                return new(saved, message);
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                _logger.LogError(ex, "Error creating address for AccountId: {AccountId}", request.AccountId);

                return new(false, _address_cannot_create);
            }
            finally
            {
                _repository.Db.Connection.Close();
            }
        }
    }
}
