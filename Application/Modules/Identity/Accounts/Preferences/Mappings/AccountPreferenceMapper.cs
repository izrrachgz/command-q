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
        [MapperIgnoreTarget(nameof(AccountPreference.Created))]
        [MapperIgnoreTarget(nameof(AccountPreference.Modified))]
        [MapperIgnoreTarget(nameof(AccountPreference.Deleted))]
        public partial AccountPreference Map(AccountPreferenceModel model);

        [MapperIgnoreSource(nameof(AccountPreference.Id))]
        [MapperIgnoreSource(nameof(AccountPreference.AccountId))]
        [MapperIgnoreSource(nameof(AccountPreference.Created))]
        [MapperIgnoreSource(nameof(AccountPreference.Modified))]
        [MapperIgnoreSource(nameof(AccountPreference.Deleted))]
        public partial AccountPreferenceModel Map(AccountPreference entity);
    }
}
