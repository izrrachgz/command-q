namespace Application.Modules.Notifications.Dtos;

/// <summary>Represents the immutable content of one queued account email.</summary>
public sealed record EmailMessageModel
{
    /// <summary>Gets the optional sender.</summary>
    public EmailAddressModel? From { get; init; }

    /// <summary>Gets the primary recipients.</summary>
    public required IReadOnlyList<EmailAddressModel> To { get; init; }

    /// <summary>Gets the carbon-copy recipients.</summary>
    public required IReadOnlyList<EmailAddressModel> Cc { get; init; }

    /// <summary>Gets the blind-carbon-copy recipients.</summary>
    public required IReadOnlyList<EmailAddressModel> Bcc { get; init; }

    /// <summary>Gets the reply-to addresses.</summary>
    public required IReadOnlyList<EmailAddressModel> ReplyTo { get; init; }

    /// <summary>Gets the message subject.</summary>
    public required string Subject { get; init; }

    /// <summary>Gets the optional plain-text body.</summary>
    public string? TextBody { get; init; }

    /// <summary>Gets the optional HTML body.</summary>
    public string? HtmlBody { get; init; }

    /// <summary>Gets the stored attachments.</summary>
    public required IReadOnlyList<EmailContentModel> Attachments { get; init; }

    /// <summary>Gets the stored inline resources.</summary>
    public required IReadOnlyList<EmailContentModel> InlineResources { get; init; }
}
