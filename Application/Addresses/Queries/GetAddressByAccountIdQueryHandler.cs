using Public;
using AutoMapper;
using Common.Cqrs;
using Domain.Entities;
using SqlKata.Execution;
using Domain.Interfaces;
using Common.Models.Response;
using Localization.Abstractions;
using Application.Addresses.Dtos;

namespace Application.Addresses.Queries
{
    public class GetAddressByAccountIdQueryHandler : IQueryHandler<GetAddressByAccountIdQuery, DetailResponse<AddressModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITextProvider _messages;
        private readonly IReadOnlyRepository _repository;

        private readonly string _record_found;
        private readonly string _record_not_found;

        public GetAddressByAccountIdQueryHandler(IMapper mapper, IReadOnlyRepository repository, ITextProvider textProvider)
        {
            _mapper = mapper;
            _repository = repository;
            _messages = textProvider;
            _record_found = _messages.Get(nameof(Messages.Record_Found));
            _record_not_found = _messages.Get(nameof(Messages.Record_Not_Found));
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
            var query = _repository.Db.Query($@"{nameof(Address)}")
                .WhereNull($@"{nameof(Address.Deleted)}")
                .Where($@"{nameof(Address.AccountId)}", request.AccountId)
                .OrderByDesc($@"{nameof(Address.Modified)}")
                .Take(1);

            var record = await query.FirstOrDefaultAsync<Address>(cancellationToken: cancellationToken);

            var result = _mapper.Map<AddressModel>(record);

            bool success = result is not null;

            string message = success ? _record_found : _record_not_found;

            return new(success, message, result);
        }
    }
}
