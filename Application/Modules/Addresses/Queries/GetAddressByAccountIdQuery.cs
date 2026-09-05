using Cqrs.Abstractions;
using Common.Models.Response;
using Application.Abstractions;
using Application.Modules.Addresses.Dtos;

namespace Application.Modules.Addresses.Queries
{
    public record GetAddressByAccountIdQuery : IQuery<DetailResponse<AddressModel>>, IAccountScopedRequest
    {
        public required Guid AccountId { get; set; }
    }
}
