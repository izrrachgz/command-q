using Application.Interfaces;
using Riok.Mapperly.Abstractions;
using Application.Modules.Contacts.Dtos;

namespace Application.Modules.Contacts.Mappings
{
    [Mapper]
    public partial class ContactMapper : IMapper<Contact, ContactModel>
    {
        [MapperIgnoreTarget(nameof(Contact.Id))]
        [MapperIgnoreTarget(nameof(Contact.AccountId))]
        [MapperIgnoreTarget(nameof(Contact.Created))]
        [MapperIgnoreTarget(nameof(Contact.Modified))]
        [MapperIgnoreTarget(nameof(Contact.Deleted))]
        public partial Contact Map(ContactModel model);

        [MapperIgnoreSource(nameof(Contact.Id))]
        [MapperIgnoreSource(nameof(Contact.AccountId))]
        [MapperIgnoreSource(nameof(Contact.Created))]
        [MapperIgnoreSource(nameof(Contact.Modified))]
        [MapperIgnoreSource(nameof(Contact.Deleted))]
        public partial ContactModel Map(Contact entity);
    }
}
