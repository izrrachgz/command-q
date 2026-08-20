using Public;
using Common.Cqrs;
using Common.Models.Response;
using Application.Abstractions;
using Persistence.Abstractions;
using Localization.Abstractions;
using Application.Modules.Addresses.Dtos;

namespace Application.Modules.Addresses.Queries
{
    internal sealed class GetAddressByAccountIdQueryHandler : IQueryHandler<GetAddressByAccountIdQuery, DetailResponse<AddressModel>>
    {
        private readonly IMapper<Address, AddressModel> _mapper;
        private readonly ITextProvider _messages;
        private readonly IReadOnlyRepository _repository;

        private readonly string _message_record_found;
        private readonly string _message_record_not_found;

        public GetAddressByAccountIdQueryHandler(IMapper<Address, AddressModel> mapper, IReadOnlyRepository repository, ITextProvider textProvider)
        {
            _mapper = mapper;
            _repository = repository;
            _messages = textProvider;
            _message_record_found = _messages.Get(nameof(Messages.Record_Found));
            _message_record_not_found = _messages.Get(nameof(Messages.Record_Not_Found));
        }

        /// <summary>
        /// Handles the retrieval of the most recently modified address associated with the specified account
        /// identifier.
        /// </summary>
        /// <param name="request">The query containing the account identifier for which to retrieve the address.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a DetailResponse<AddressModel>
        /// with the address details if found; otherwise, the response indicates that no address was found.</returns>
        public async Task<DetailResponse<AddressModel>> Handle(GetAddressByAccountIdQuery request, CancellationToken cancellationToken)
        {
            var query = _repository.Query($@"{nameof(Address)}")
                .WhereNull($@"{nameof(Address.DeletedUtc)}")
                .Where($@"{nameof(Address.AccountId)}", request.AccountId)
                .OrderByDesc($@"{nameof(Address.ModifiedUtc)}")
                .Take(1);

            var record = await _repository.FirstOrDefaultAsync<Address>(query, cancellationToken);

            if (record is null)
                return new(false, _message_record_not_found, null);

            return new(true, _message_record_found, _mapper.Map(record));
        }
    }
}
