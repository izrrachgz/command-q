using AutoMapper;
using Common.Cqrs;
using Common.Models.Response;
using Application.Accounts.Dtos;
using Domain.Entities;

namespace Application.Accounts.Queries
{
    public class GetAccountByUsernameQueryHandler : IQueryHandler<GetAccountByUsernameQuery, DetailResponse<AccountModel>>
    {
        private readonly IMapper _mapper;

        public GetAccountByUsernameQueryHandler(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<DetailResponse<AccountModel>> Handle(GetAccountByUsernameQuery request, CancellationToken cancellationToken)
        {
            var account = new Account();

            var result = _mapper.Map<AccountModel>(account);

            return new(true, "Authorized", result);
        }
    }
}
