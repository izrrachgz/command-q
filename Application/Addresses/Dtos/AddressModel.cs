namespace Application.Addresses.Dtos
{
    public sealed record AddressModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Street { get; set; }
        public string? ExternalNumber { get; set; }
        public string? InternalNumber { get; set; }
        public string? References { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? Phone { get; set; }
        public string? EmailAddress { get; set; }
    }
}
