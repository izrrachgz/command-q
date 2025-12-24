using Public;
using AutoMapper;
using Common.Hash;
using Common.Cqrs;
using Domain.Entities;
using SqlKata.Execution;
using Domain.Interfaces;
using Common.Models.Response;
using Application.Accounts.Dtos;
using Localization.Abstractions;

namespace Application.Accounts.Queries
{
    public class GetAccountByUsernameQueryHandler : IQueryHandler<GetAccountByUsernameQuery, DetailResponse<AccountModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITextProvider _messages;
        private readonly IReadOnlyRepository _repository;

        private readonly string _record_found;
        private readonly string _record_not_found;

        public GetAccountByUsernameQueryHandler(IMapper mapper, IReadOnlyRepository repository, ITextProvider textProvider)
        {
            _mapper = mapper;
            _repository = repository;
            _messages = textProvider;
            _record_found = _messages.Get(nameof(Messages.Record_Found));
            _record_not_found = _messages.Get(nameof(Messages.Record_Not_Found));
        }

        /// <summary>
        /// Handles a request to retrieve account details by username and verify the provided password.
        /// </summary>
        /// <remarks>If the username does not exist or the password is incorrect, the response indicates
        /// failure and the account details are not included.</remarks>
        /// <param name="request">The query containing the username and password to authenticate and retrieve the account details.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a DetailResponse<AccountModel>
        /// indicating whether the account was found and authenticated, along with the account details if successful.</returns>
        public async Task<DetailResponse<AccountModel>> Handle(GetAccountByUsernameQuery request, CancellationToken cancellationToken)
        {
            string hashPassword = Hash.ComputeSHA256(request.Password);

            var query = _repository.Db.Query($@"{nameof(Account)}")
                .WhereNull($@"{nameof(Account.Deleted)}")
                .Where($@"{nameof(Account.Username)}", request.Username)
                .OrderByDesc($@"{nameof(Account.Modified)}")
                .Take(1);

            var record = await query.FirstOrDefaultAsync<Account>(cancellationToken: cancellationToken);

            var result = _mapper.Map<AccountModel>(record);

            bool success = result is not null && Hash.CompareSHA256(record.Password, hashPassword);

            string message = success ? _record_found : _record_not_found;

            return new(success, message, result);
        }
    }
}
