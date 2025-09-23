namespace Domain.Entities
{
    public class RegisterEvent : BaseEntity
    {
        public required long RegisterId { get; set; }
        public required long EventId { get; set; }
    }
}
