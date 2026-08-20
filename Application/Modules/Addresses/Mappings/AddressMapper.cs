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
        [MapperIgnoreTarget(nameof(Address.CreatedUtc))]
        [MapperIgnoreTarget(nameof(Address.ModifiedUtc))]
        [MapperIgnoreTarget(nameof(Address.DeletedUtc))]
        public partial Address Map(AddressModel model);

        [MapperIgnoreSource(nameof(Address.Id))]
        [MapperIgnoreSource(nameof(Address.AccountId))]
        [MapperIgnoreSource(nameof(Address.CreatedUtc))]
        [MapperIgnoreSource(nameof(Address.ModifiedUtc))]
        [MapperIgnoreSource(nameof(Address.DeletedUtc))]
        public partial AddressModel Map(Address entity);
    }
}
