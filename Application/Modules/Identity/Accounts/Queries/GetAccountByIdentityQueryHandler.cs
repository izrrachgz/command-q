using Public;
using Common.Cqrs;
using Application.Abstractions;
using Common.Models.Response;
using Persistence.Abstractions;
using Localization.Abstractions;
using Application.Modules.Identity.Accounts.Dtos;

namespace Application.Modules.Identity.Accounts.Queries
{
    public sealed class GetAccountByIdentityQueryHandler(
        IMapper<Account, AccountModel> mapper,
        IReadOnlyRepository repository,
        ITextProvider textProvider)
        : IQueryHandler<GetAccountByIdentityQuery, DetailResponse<AccountModel>>
    {
        private readonly string _messageRecordFound = textProvider.Get(nameof(Messages.Record_Found));
        private readonly string _messageRecordNotFound = textProvider.Get(nameof(Messages.Record_Not_Found));

        public async Task<DetailResponse<AccountModel>> Handle(GetAccountByIdentityQuery request, CancellationToken cancellationToken)
        {
            var query = repository.Query($"{nameof(AccountIdentity)} as identity")
                .Join($"{nameof(Account)} as account", "account.Id", "identity.AccountId")
                .Select("account.*")
                .WhereNull("identity.Deleted")
                .WhereNull("account.Deleted")
                .Where("identity.Issuer", request.Issuer)
                .Where("identity.Subject", request.Subject)
                .Limit(1);

            var account = await repository.FirstOrDefaultAsync<Account>(query, cancellationToken);

            return account is null
                ? new(false, _messageRecordNotFound, null)
                : new(true, _messageRecordFound, mapper.Map(account));
        }
    }
}
