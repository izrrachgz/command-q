using Application.Abstractions;
using Riok.Mapperly.Abstractions;
using Application.Modules.Identity.Accounts.Preferences.Dtos;

namespace Application.Modules.Identity.Accounts.Preferences.Mappings
{
    [Mapper]
    internal partial class AccountPreferenceMapper : IMapper<AccountPreference, AccountPreferenceModel>
    {
        [MapperIgnoreTarget(nameof(AccountPreference.Id))]
        [MapperIgnoreTarget(nameof(AccountPreference.AccountId))]
        [MapperIgnoreTarget(nameof(AccountPreference.CreatedUtc))]
        [MapperIgnoreTarget(nameof(AccountPreference.ModifiedUtc))]
        [MapperIgnoreTarget(nameof(AccountPreference.DeletedUtc))]
        public partial AccountPreference Map(AccountPreferenceModel model);

        [MapperIgnoreSource(nameof(AccountPreference.Id))]
        [MapperIgnoreSource(nameof(AccountPreference.AccountId))]
        [MapperIgnoreSource(nameof(AccountPreference.CreatedUtc))]
        [MapperIgnoreSource(nameof(AccountPreference.ModifiedUtc))]
        [MapperIgnoreSource(nameof(AccountPreference.DeletedUtc))]
        public partial AccountPreferenceModel Map(AccountPreference entity);
    }
}
