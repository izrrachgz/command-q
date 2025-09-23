namespace Domain.Entities
{
    public class Register : BaseEntity
    {
        public required long AccountId { get; set; }
        public required TrackingAction Action { get; set; }
    }
}
