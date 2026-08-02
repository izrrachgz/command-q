namespace Application.Modules.Identity.AccountActivity.Dtos
{
    public sealed class AccountActivityModel
    {
        public long Id { get; set; }

        public Guid AccountId { get; set; }

        public string Module { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string? EntityType { get; set; }

        public string? EntityId { get; set; }

        public string Message { get; set; } = string.Empty;

        public string? MetadataJson { get; set; }

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public DateTime CreatedUtc { get; set; }
    }
}
