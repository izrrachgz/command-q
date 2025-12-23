using Application.Contacts.Dtos;
using Application.Addresses.Dtos;

namespace Application.Accounts.Dtos
{
    public sealed record AccountModel
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Dictionary<string, AddressModel> Addresses { get; set; } = [];
        public Dictionary<string, ContactModel> Contacts { get; set; } = [];
    }
}
