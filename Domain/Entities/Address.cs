namespace Domain.Entities
{
    public class Address : BaseEntity
    {
        public required long AccountId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string Street { get; set; }
        public string? ExternalNumber { get; set; }
        public string? InternalNumber { get; set; }
        public string? References { get; set; }
        public required string City { get; set; }
        public required string PostalCode { get; set; }
        public required string Country { get; set; }
        public string? Phone { get; set; }
        public string? EmailAddress { get; set; }
    }
}
