namespace Application.Addresses.Dtos
{
    public sealed record AddressModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Line1 { get; set; } = string.Empty;
        public string? Line2 { get; set; } = string.Empty;
        public string? ExternalNumber { get; set; }
        public string? InternalNumber { get; set; }
        public string? References { get; set; }
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? EmailAddress { get; set; }
    }
}
