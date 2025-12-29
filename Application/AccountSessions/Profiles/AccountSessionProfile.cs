using AutoMapper;
using Domain.Entities;
using Application.AccountSessions.Dtos;

namespace Application.AccountSessions.Profiles
{
    internal sealed class AccountSessionProfile : Profile
    {
        public AccountSessionProfile()
        {
            //Mapping from Account Session entity to Account Session Model DTO
            CreateMap<AccountSession, AccountSessionModel>();
        }
    }
}
