namespace Application.Modules.Notifications.Dtos;

/// <summary>Represents an email address displayed in an account notification.</summary>
public sealed record EmailAddressModel
{
    /// <summary>Gets the mailbox address.</summary>
    public required string Address { get; init; }

    /// <summary>Gets the optional display name.</summary>
    public string? Name { get; init; }
}
