namespace Api.Modules.Contacts.Requests
{
    /// <summary>
    /// Describes a contact to create for the authenticated account.
    /// </summary>
    public sealed record CreateContactRequest
    {
        public required string Name { get; init; }
        public string? Description { get; init; }
        public string? EmailAddress { get; init; }
        public string? PhoneNumber { get; init; }
    }
}
