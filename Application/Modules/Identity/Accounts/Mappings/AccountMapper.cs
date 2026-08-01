using Application.Interfaces;
using Riok.Mapperly.Abstractions;
using Application.Modules.Identity.Accounts.Dtos;

namespace Application.Modules.Identity.Accounts.Mappings
{
    [Mapper]
    public partial class AccountMapper : IMapper<Account, AccountModel>
    {
        [MapperIgnoreTarget(nameof(Account.Created))]
        [MapperIgnoreTarget(nameof(Account.Modified))]
        [MapperIgnoreTarget(nameof(Account.Deleted))]
        [MapperIgnoreSource(nameof(AccountModel.Addresses))]
        [MapperIgnoreSource(nameof(AccountModel.Contacts))]
        public partial Account Map(AccountModel model);

        [MapperIgnoreSource(nameof(Account.Created))]
        [MapperIgnoreSource(nameof(Account.Modified))]
        [MapperIgnoreSource(nameof(Account.Deleted))]
        [MapperIgnoreTarget(nameof(AccountModel.Addresses))]
        [MapperIgnoreTarget(nameof(AccountModel.Contacts))]
        public partial AccountModel Map(Account entity);

        private static string[] MapRoles(string roles)
            => roles?.Split([','], StringSplitOptions.RemoveEmptyEntries) ?? [];

        private static string MapRoles(string[] roles)
            => roles == null ? string.Empty : string.Join(',', roles);
    }
}
