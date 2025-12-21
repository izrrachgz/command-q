namespace Domain.Entities
{
    public class Register : BaseEntity
    {
        public required Guid AccountId { get; set; }
        public required TrackingAction Action { get; set; }
    }
}
