using Application.Abstractions;
using Riok.Mapperly.Abstractions;
using Application.Modules.Identity.AccountActivity.Dtos;

namespace Application.Modules.Identity.AccountActivity.Mappings
{
    [Mapper]
    internal partial class AccountActivityMapper : IMapper<Domain.Modules.Identity.AccountActivity, AccountActivityModel>
    {
        [MapperIgnoreSource(nameof(Domain.Modules.Identity.AccountActivity.ModifiedUtc))]
        [MapperIgnoreSource(nameof(Domain.Modules.Identity.AccountActivity.DeletedUtc))]
        public partial AccountActivityModel Map(Domain.Modules.Identity.AccountActivity entity);

        [MapperIgnoreTarget(nameof(Domain.Modules.Identity.AccountActivity.ModifiedUtc))]
        [MapperIgnoreTarget(nameof(Domain.Modules.Identity.AccountActivity.DeletedUtc))]
        public partial Domain.Modules.Identity.AccountActivity Map(AccountActivityModel model);
    }
}
