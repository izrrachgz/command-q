using AutoMapper;
using Common.Hash;
using Common.Cqrs;
using Domain.Entities;
using SqlKata.Execution;
using Domain.Interfaces;
using Application.Constants;
using Common.Models.Response;
using Application.Accounts.Dtos;

namespace Application.Accounts.Queries
{
    public class GetAccountByUsernameQueryHandler : IQueryHandler<GetAccountByUsernameQuery, DetailResponse<AccountModel>>
    {
        private readonly IMapper _mapper;
        private readonly IReadOnlyRepository _repository;

        public GetAccountByUsernameQueryHandler(IMapper mapper, IReadOnlyRepository repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<DetailResponse<AccountModel>> Handle(GetAccountByUsernameQuery request, CancellationToken cancellationToken)
        {
            string hashPassword = Hash.ComputeSHA256(request.Password);

            var query = _repository.Db.Query($@"{nameof(Account)}")
                .WhereNotNull($@"{nameof(Account.Deleted)}")
                .Where($@"{nameof(Account.Username)}", request.Username)
                .OrderByDesc($@"{nameof(Account.Modified)}")
                .Take(1);

            var record = await query.FirstOrDefaultAsync<Account>(cancellationToken: cancellationToken);

            var result = _mapper.Map<AccountModel>(record);

            bool success = result is not null && Hash.CompareSHA256(record.Password, hashPassword);

            string message = success ? ApplicationMessages.RecordFound : ApplicationMessages.RecordNotFound;

            return new(success, message, result);
        }
    }
}
