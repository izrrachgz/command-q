#nullable enable
using Domain.Entities;
namespace Domain.Modules.Contacts
{
    public class Contact : BaseEntity
    {
        public Guid AccountId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
