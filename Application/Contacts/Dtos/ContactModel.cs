namespace Application.Contacts.Dtos
{
    public sealed record ContactModel
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
