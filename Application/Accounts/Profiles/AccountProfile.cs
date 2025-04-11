using AutoMapper;
using Domain.Entities;
using Application.Accounts.Dtos;

namespace Application.Accounts.Profiles
{
    internal sealed class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<Account, AccountModel>();
        }
    }
}
