namespace Email.Models
{
    /// <summary>
    /// Represents an email mailbox and its optional display name.
    /// </summary>
    public sealed record EmailAddress(string Address, string? Name = null);
}
