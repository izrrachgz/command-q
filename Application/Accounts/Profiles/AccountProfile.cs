using AutoMapper;
using Domain.Entities;
using Application.Accounts.Dtos;

namespace Application.Accounts.Profiles
{
    internal sealed class AccountProfile : Profile
    {
        public AccountProfile()
        {
            //Mapping from Account entity to AccountModel DTO
            CreateMap<Account, AccountModel>()
                .ForMember(
                    d => d.Roles,
                    s => s.MapFrom(src => src.Roles.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                );

            //Mapping from AccountModel DTO to Account entity
            CreateMap<AccountModel, Account>()
                .ForMember(
                    d => d.Roles,
                    s => s.MapFrom(src => string.Join(',', src.Roles))
                );
        }
    }
}
