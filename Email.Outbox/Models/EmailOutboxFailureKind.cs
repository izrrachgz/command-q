namespace Email.Outbox.Models
{
    public enum EmailOutboxFailureKind
    {
        Disabled = 1,
        Authentication = 2,
        Connectivity = 3,
        TransientSmtp = 4,
        PermanentSmtp = 5,
        Protocol = 6,
        Serialization = 7,
        Validation = 8,
        Unknown = 9
    }
}
