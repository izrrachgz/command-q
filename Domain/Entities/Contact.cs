#nullable enable
namespace Domain.Entities
{
    public class Contact : BaseEntity
    {
        public required long AccountId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
