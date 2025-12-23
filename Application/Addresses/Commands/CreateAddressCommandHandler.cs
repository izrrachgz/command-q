using Common.Cqrs;
using Domain.Entities;
using Domain.Interfaces;
using SqlKata.Execution;
using Application.Constants;
using Common.Models.Response;
using Microsoft.Extensions.Logging;

namespace Application.Addresses.Commands
{
    public class CreateAddressCommandHandler : ICommandHandler<CreateAddressCommand, BasicResponse>
    {
        private readonly IWritableRepository _repository;
        private readonly ILogger<CreateAddressCommandHandler> _logger;

        public CreateAddressCommandHandler(IWritableRepository repository, ILogger<CreateAddressCommandHandler> logger)
        {
            _logger = logger;
            _repository = repository;
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

                if (saved)
                    transaction.Commit();
                else
                    transaction.Rollback();

                return new(saved, saved ? ApplicationMessages.AccountSaved : ApplicationMessages.CannotCreateAccount);
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                _logger.LogError(ex, "Error creating address for AccountId: {AccountId}", request.AccountId);

                return new(false, @"Cannot create address.");
            }
            finally
            {
                _repository.Db.Connection.Close();
            }
        }
    }
}
