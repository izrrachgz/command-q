using Public;
using AutoMapper;
using Common.Cqrs;
using Domain.Entities;
using SqlKata.Execution;
using Domain.Interfaces;
using Common.Models.Response;
using Application.Accounts.Dtos;
using Localization.Abstractions;

namespace Application.Accounts.Queries
{
    public class GetAccountByIdQueryHandler : IQueryHandler<GetAccountByIdQuery, DetailResponse<AccountModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITextProvider _messages;
        private readonly IReadOnlyRepository _repository;

        private readonly string _message_record_found;
        private readonly string _message_record_not_found;

        public GetAccountByIdQueryHandler(IMapper mapper, IReadOnlyRepository repository, ITextProvider textProvider)
        {
            _mapper = mapper;
            _repository = repository;
            _messages = textProvider;
            _message_record_found = _messages.Get(nameof(Messages.Record_Found));
            _message_record_not_found = _messages.Get(nameof(Messages.Record_Not_Found));
        }

        /// <summary>
        /// Handles the retrieval of an account by account id and returns detailed information about the account.
        /// </summary>
        /// <param name="request">The query containing the account id of the account to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a DetailResponse<AccountModel>
        /// with the account details if found; otherwise, the result indicates that no matching account was found.</returns>
        public async Task<DetailResponse<AccountModel>> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
        {
            var query = _repository.Db.Query($@"{nameof(Account)}")
                .WhereNull($@"{nameof(Account.Deleted)}")
                .Where($@"{nameof(Account.Id)}", request.AccountId)
                .Limit(1);

            var record = await query.FirstOrDefaultAsync<Account>(cancellationToken: cancellationToken);

            var result = _mapper.Map<AccountModel>(record);

            bool success = result is not null;

            string message = success ? _message_record_found : _message_record_not_found;

            return new(success, message, result);
        }
    }
}
