namespace Domain.Entities
{
    public class Account : BaseEntity
    {
        public required Guid Identifier { get; set; }
        public required string Username {  get; set; }
        public required string Password {  get; set; }
    }
}
