namespace Application.Contacts.Dtos
{
    public sealed record ContactModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = null;
        public string? EmailAddress { get; set; } = null;
        public string? PhoneNumber { get; set; } = null;
    }
}
