using AutoMapper;
using Domain.Entities;
using Application.Contacts.Dtos;

namespace Application.Contacts.Profiles
{
    internal sealed class ContactProfile : Profile
    {
        public ContactProfile()
        {
            CreateMap<Contact, ContactModel>();
        }
    }
}
