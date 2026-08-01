using Common.Cqrs;
using Common.Models.Response;
using Application.Modules.Addresses.Dtos;

namespace Application.Modules.Addresses.Queries
{
    public record GetAddressByAccountIdQuery : IQuery<DetailResponse<AddressModel>>
    {
        public required Guid AccountId { get; set; }
    }
}
