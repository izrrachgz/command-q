using Domain.Entities;
using Application.Interfaces;
using Application.Files.Dtos;
using Riok.Mapperly.Abstractions;

namespace Application.Files.Mappings
{
    [Mapper]
    public partial class AccountFileMapper : IMapper<AccountFile, AccountFileModel>
    {
        [MapperIgnoreSource(nameof(AccountFile.AccountId))]
        [MapperIgnoreSource(nameof(AccountFile.Path))]
        [MapperIgnoreSource(nameof(AccountFile.Deleted))]
        public partial AccountFileModel Map(AccountFile entity);

        public AccountFile Map(AccountFileModel model)
        {
            throw new NotSupportedException($@"{nameof(AccountFileModel)} cannot create {nameof(AccountFile)} because file ownership and storage path are internal values.");
        }
    }
}
