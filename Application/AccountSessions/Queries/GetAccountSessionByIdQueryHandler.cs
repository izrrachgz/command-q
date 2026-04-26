using Public;
using Common.Cqrs;
using Domain.Entities;
using Application.Interfaces;
using Common.Models.Response;
using Localization.Abstractions;
using Application.Accounts.Services;
using Application.AccountSessions.Dtos;

namespace Application.AccountSessions.Queries
{
    public class GetAccountSessionByIdQueryHandler : IQueryHandler<GetAccountSessionByIdQuery, DetailResponse<AccountSessionModel>>
    {
        private readonly IMapper<AccountSession, AccountSessionModel> _mapper;
        private readonly ITextProvider _messages;
        private readonly AccountService _accountService;

        private readonly string _message_record_found;
        private readonly string _message_record_not_found;

        public GetAccountSessionByIdQueryHandler(IMapper<AccountSession, AccountSessionModel> mapper, ITextProvider textProvider, AccountService accountService)
        {
            _mapper = mapper;
            _messages = textProvider;
            _accountService = accountService;
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
        public async Task<DetailResponse<AccountSessionModel>> Handle(GetAccountSessionByIdQuery request, CancellationToken cancellationToken)
        {
            var account = await _accountService.GetAccountSession(request.AccountId, cancellationToken);

            if (account is null)
                return new(false, _message_record_not_found, null);

            return new(true, _message_record_found, _mapper.Map(account));
        }
    }
}
