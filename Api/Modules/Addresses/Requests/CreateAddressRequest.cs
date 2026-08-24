namespace Api.Modules.Addresses.Requests
{
    /// <summary>
    /// Describes an address to create for the authenticated account.
    /// </summary>
    public sealed record CreateAddressRequest
    {
        public required string Name { get; init; }
        public string? Description { get; init; }
        public required string Line1 { get; init; }
        public string? Line2 { get; init; }
        public string? ExternalNumber { get; init; }
        public string? InternalNumber { get; init; }
        public string? References { get; init; }
        public required string City { get; init; }
        public required string PostalCode { get; init; }
        public required string Country { get; init; }
        public string? Phone { get; init; }
        public string? EmailAddress { get; init; }
    }
}
