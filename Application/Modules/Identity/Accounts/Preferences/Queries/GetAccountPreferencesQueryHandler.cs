using Public;
using Cqrs.Abstractions;
using Common.Models.Response;
using Application.Abstractions;
using Localization.Abstractions;
using Application.Modules.Identity.Accounts.Preferences.Dtos;
using Application.Modules.Identity.Accounts.Preferences.Services;

namespace Application.Modules.Identity.Accounts.Preferences.Queries
{
    internal sealed class GetAccountPreferencesQueryHandler(
        AccountPreferenceService service,
        IMapper<AccountPreference, AccountPreferenceModel> mapper,
        ITextProvider messages) : IQueryHandler<GetAccountPreferencesQuery, DetailResponse<AccountPreferenceModel>>
    {
        public async Task<DetailResponse<AccountPreferenceModel>> Handle(
            GetAccountPreferencesQuery request,
            CancellationToken cancellationToken)
        {
            var preferences = await service.Get(request.AccountId, cancellationToken);
            return preferences is null
                ? new(false, messages.Get(nameof(Messages.Record_Not_Found)), null)
                : new(true, messages.Get(nameof(Messages.Record_Found)), mapper.Map(preferences));
        }
    }
}
