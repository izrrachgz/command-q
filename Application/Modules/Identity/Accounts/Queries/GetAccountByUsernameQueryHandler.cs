using Public;
using Common.Cqrs;
using Application.Abstractions;
using Common.Models.Response;
using Persistence.Abstractions;
using Localization.Abstractions;
using Application.Modules.Identity.Accounts.Dtos;

namespace Application.Modules.Identity.Accounts.Queries
{
    public class GetAccountByUsernameQueryHandler : IQueryHandler<GetAccountByUsernameQuery, DetailResponse<AccountModel>>
    {
        private readonly IMapper<Account, AccountModel> _mapper;
        private readonly ITextProvider _messages;
        private readonly IReadOnlyRepository _repository;

        private readonly string _message_record_found;
        private readonly string _message_record_not_found;

        public GetAccountByUsernameQueryHandler(IMapper<Account, AccountModel> mapper, IReadOnlyRepository repository, ITextProvider textProvider)
        {
            _mapper = mapper;
            _repository = repository;
            _messages = textProvider;
            _message_record_found = _messages.Get(nameof(Messages.Record_Found));
            _message_record_not_found = _messages.Get(nameof(Messages.Record_Not_Found));
        }

        /// <summary>
        /// Handles the retrieval of an account by username and returns detailed information about the account.
        /// </summary>
        /// <param name="request">The query containing the username of the account to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a DetailResponse<AccountModel>
        /// with the account details if found; otherwise, the result indicates that no matching account was found.</returns>
        public async Task<DetailResponse<AccountModel>> Handle(GetAccountByUsernameQuery request, CancellationToken cancellationToken)
        {
            var query = _repository.Query($@"{nameof(Account)}")
                .WhereNull($@"{nameof(Account.Deleted)}")
                .Where($@"{nameof(Account.Username)}", request.Username)
                .Limit(1);

            var record = await _repository.FirstOrDefaultAsync<Account>(query, cancellationToken);

            if (record is null)
                return new(false, _message_record_not_found, null);

            return new(true, _message_record_found, _mapper.Map(record));
        }
    }
}
