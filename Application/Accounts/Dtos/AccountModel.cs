namespace Application.Accounts.Dtos
{
    public sealed record AccountModel
    {
        public Guid? Identifier { get; set; }
        public string? Username { get; set; }
    }
}
