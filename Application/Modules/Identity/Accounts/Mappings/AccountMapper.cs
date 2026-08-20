using Application.Abstractions;
using Riok.Mapperly.Abstractions;
using Application.Modules.Identity.Accounts.Dtos;

namespace Application.Modules.Identity.Accounts.Mappings
{
    [Mapper]
    internal partial class AccountMapper : IMapper<Account, AccountModel>
    {
        [MapperIgnoreTarget(nameof(Account.CreatedUtc))]
        [MapperIgnoreTarget(nameof(Account.ModifiedUtc))]
        [MapperIgnoreTarget(nameof(Account.DeletedUtc))]
        [MapperIgnoreSource(nameof(AccountModel.Addresses))]
        [MapperIgnoreSource(nameof(AccountModel.Contacts))]
        public partial Account Map(AccountModel model);

        [MapperIgnoreSource(nameof(Account.CreatedUtc))]
        [MapperIgnoreSource(nameof(Account.ModifiedUtc))]
        [MapperIgnoreSource(nameof(Account.DeletedUtc))]
        [MapperIgnoreTarget(nameof(AccountModel.Addresses))]
        [MapperIgnoreTarget(nameof(AccountModel.Contacts))]
        public partial AccountModel Map(Account entity);
    }
}
