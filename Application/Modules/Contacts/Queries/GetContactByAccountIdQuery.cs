using Cqrs.Abstractions;
using Common.Models.Response;
using Application.Abstractions;
using Application.Modules.Contacts.Dtos;

namespace Application.Modules.Contacts.Queries
{
    public record GetContactByAccountIdQuery : IQuery<DetailResponse<ContactModel>>, IAccountScopedRequest
    {
        public required Guid AccountId { get; set; }
    }
}
