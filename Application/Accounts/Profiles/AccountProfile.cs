using AutoMapper;
using Domain.Entities;
using Application.Accounts.Dtos;

namespace Application.Accounts.Profiles
{
    internal sealed class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<Account, AccountModel>()
                .ForMember(
                    d => d.Roles,
                    s => s.MapFrom(src => src.Roles.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                );
        }
    }
}
