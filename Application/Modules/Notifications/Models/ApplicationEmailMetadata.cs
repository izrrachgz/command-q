namespace Application.Modules.Notifications.Models
{
    public sealed record ApplicationEmailMetadata
    {
        public string? IdempotencyKey { get; init; }

        public string? AccountReference { get; init; }

        public string? CorrelationId { get; init; }
    }
}
