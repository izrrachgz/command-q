namespace Email.Outbox.Models
{
    public enum EmailOutboxStatus
    {
        Pending = 1,
        Processing = 2,
        Sent = 3,
        DeadLetter = 4
    }
}
