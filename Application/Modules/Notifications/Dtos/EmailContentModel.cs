namespace Application.Modules.Notifications.Dtos;

/// <summary>Represents an attachment or inline resource stored with a queued email snapshot.</summary>
public sealed record EmailContentModel
{
    /// <summary>Gets the original file name.</summary>
    public required string FileName { get; init; }

    /// <summary>Gets the optional MIME content type.</summary>
    public string? ContentType { get; init; }

    /// <summary>Gets the optional content identifier used by inline resources.</summary>
    public string? ContentId { get; init; }

    /// <summary>Gets the stored content size without exposing its bytes.</summary>
    public required long Size { get; init; }
}
