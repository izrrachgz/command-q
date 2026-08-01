using Application.Interfaces;
using Riok.Mapperly.Abstractions;
using Application.Modules.Identity.AccountSessions.Dtos;

namespace Application.Modules.Identity.AccountSessions.Mappings
{
    [Mapper]
    public partial class AccountSessionMapper : IMapper<AccountSession, AccountSessionModel>
    {
        [MapperIgnoreTarget(nameof(AccountSession.Created))]
        [MapperIgnoreTarget(nameof(AccountSession.Modified))]
        [MapperIgnoreTarget(nameof(AccountSession.Deleted))]
        [MapperIgnoreSource(nameof(AccountSessionModel.SessionId))]
        [MapperIgnoreSource(nameof(AccountSessionModel.UniqueName))]
        [MapperIgnoreSource(nameof(AccountSessionModel.Roles))]
        public partial AccountSession Map(AccountSessionModel model);

        [MapperIgnoreSource(nameof(AccountSession.Created))]
        [MapperIgnoreSource(nameof(AccountSession.Modified))]
        [MapperIgnoreSource(nameof(AccountSession.Deleted))]
        [MapperIgnoreTarget(nameof(AccountSessionModel.SessionId))]
        [MapperIgnoreTarget(nameof(AccountSessionModel.UniqueName))]
        [MapperIgnoreTarget(nameof(AccountSessionModel.Roles))]
        public partial AccountSessionModel Map(AccountSession entity);
    }
}
