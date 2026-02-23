using Common.Cqrs;
using Common.Models.Response;
using Application.Addresses.Dtos;

namespace Application.Addresses.Queries
{
    public record GetAddressByAccountIdQuery : IQuery<DetailResponse<AddressModel>>
    {
        public required Guid AccountId { get; set; }
    }
}
