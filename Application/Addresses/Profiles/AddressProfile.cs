using AutoMapper;
using Domain.Entities;
using Application.Addresses.Dtos;

namespace Application.Addresses.Profiles
{
    internal sealed class AddressProfile : Profile
    {
        public AddressProfile()
        {
            CreateMap<Address, AddressModel>();
        }
    }
}
