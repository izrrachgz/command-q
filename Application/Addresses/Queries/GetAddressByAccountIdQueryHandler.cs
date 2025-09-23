using AutoMapper;
using Common.Cqrs;
using Domain.Entities;
using SqlKata.Execution;
using Domain.Interfaces;
using Application.Constants;
using Common.Models.Response;
using Application.Addresses.Dtos;

namespace Application.Addresses.Queries
{
    public class GetAddressByAccountIdQueryHandler : IQueryHandler<GetAddressByAccountIdQuery, DetailResponse<AddressModel>>
    {
        private readonly IMapper _mapper;
        private readonly IReadOnlyRepository _repository;

        public GetAddressByAccountIdQueryHandler(IMapper mapper, IReadOnlyRepository repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

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

            string message = success ? ApplicationMessages.RecordFound : ApplicationMessages.RecordNotFound;

            return new(success, message, result);
        }
    }
}
