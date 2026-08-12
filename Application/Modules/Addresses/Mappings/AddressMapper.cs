using Domain.Entities;
using Application.Abstractions;
using Riok.Mapperly.Abstractions;
using Application.Modules.Addresses.Dtos;

namespace Application.Modules.Addresses.Mappings
{
    [Mapper]
    internal partial class AddressMapper : IMapper<Address, AddressModel>
    {
        [MapperIgnoreTarget(nameof(Address.Id))]
        [MapperIgnoreTarget(nameof(Address.AccountId))]
        [MapperIgnoreTarget(nameof(Address.Created))]
        [MapperIgnoreTarget(nameof(Address.Modified))]
        [MapperIgnoreTarget(nameof(Address.Deleted))]
        public partial Address Map(AddressModel model);

        [MapperIgnoreSource(nameof(Address.Id))]
        [MapperIgnoreSource(nameof(Address.AccountId))]
        [MapperIgnoreSource(nameof(Address.Created))]
        [MapperIgnoreSource(nameof(Address.Modified))]
        [MapperIgnoreSource(nameof(Address.Deleted))]
        public partial AddressModel Map(Address entity);
    }
}
