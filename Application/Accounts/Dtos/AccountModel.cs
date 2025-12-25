using Application.Contacts.Dtos;
using Application.Addresses.Dtos;

namespace Application.Accounts.Dtos
{
    public sealed record AccountModel
    {
        public Guid Id { get; init; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; init; } = null!;
        public int TokenVersion { get; set; }
        public string[] Roles { get; init; } = [];
        public byte FailedLoginAttempts { get; set; }
        public DateTime? LockedUntilUtc { get; set; }
        public Dictionary<string, AddressModel> Addresses { get; set; } = [];
        public Dictionary<string, ContactModel> Contacts { get; set; } = [];
    }
}
