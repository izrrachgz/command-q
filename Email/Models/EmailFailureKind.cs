namespace Email.Models
{
    /// <summary>
    /// Describes the action a caller can reasonably take after a delivery failure.
    /// </summary>
    public enum EmailFailureKind
    {
        Disabled = 0,
        Authentication = 1,
        Connectivity = 2,
        TransientSmtp = 3,
        PermanentSmtp = 4,
        Protocol = 5,
        Unknown = 6
    }
}
