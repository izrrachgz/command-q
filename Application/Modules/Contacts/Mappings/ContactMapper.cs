using Application.Abstractions;
using Riok.Mapperly.Abstractions;
using Application.Modules.Contacts.Dtos;

namespace Application.Modules.Contacts.Mappings
{
    [Mapper]
    internal partial class ContactMapper : IMapper<Contact, ContactModel>
    {
        [MapperIgnoreTarget(nameof(Contact.Id))]
        [MapperIgnoreTarget(nameof(Contact.AccountId))]
        [MapperIgnoreTarget(nameof(Contact.CreatedUtc))]
        [MapperIgnoreTarget(nameof(Contact.ModifiedUtc))]
        [MapperIgnoreTarget(nameof(Contact.DeletedUtc))]
        public partial Contact Map(ContactModel model);

        [MapperIgnoreSource(nameof(Contact.Id))]
        [MapperIgnoreSource(nameof(Contact.AccountId))]
        [MapperIgnoreSource(nameof(Contact.CreatedUtc))]
        [MapperIgnoreSource(nameof(Contact.ModifiedUtc))]
        [MapperIgnoreSource(nameof(Contact.DeletedUtc))]
        public partial ContactModel Map(Contact entity);
    }
}
