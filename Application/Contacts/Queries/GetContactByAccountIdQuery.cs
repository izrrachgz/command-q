using Common.Cqrs;
using Common.Models.Response;
using Application.Contacts.Dtos;

namespace Application.Contacts.Queries
{
    public record GetContactByAccountIdQuery : IQuery<DetailResponse<ContactModel>>
    {
        public required Guid AccountId { get; set; }
    }
}
