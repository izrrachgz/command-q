namespace Api.Modules.Notifications.Requests;

/// <summary>Browser-facing filters for administrator email history.</summary>
public sealed record NotificationEmailSearchRequest
{
    public string? AccountReference { get; init; }
    public string? SearchTerm { get; init; }
    public string? Recipient { get; init; }
    public string? Status { get; init; }
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public int Page { get; init; } = 1;
    public int Size { get; init; } = Email.Client.EmailQueueConstants.DefaultPageSize;
}
