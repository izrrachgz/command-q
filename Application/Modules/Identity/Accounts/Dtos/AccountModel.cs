using Application.Modules.Contacts.Dtos;
using Application.Modules.Addresses.Dtos;

namespace Application.Modules.Identity.Accounts.Dtos
{
    public sealed record AccountModel
    {
        public Guid Id { get; init; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; init; } = null!;
        public string[] Roles { get; set; } = [];
        public Dictionary<string, AddressModel> Addresses { get; set; } = [];
        public Dictionary<string, ContactModel> Contacts { get; set; } = [];
    }
}
