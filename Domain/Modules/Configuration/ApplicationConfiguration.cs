using Domain.Entities;

namespace Domain.Modules.Configuration
{
    /// <summary>Represents an immutable snapshot of effective application settings recorded at startup or by an administrator.</summary>
    public sealed class ApplicationConfiguration : BaseEntity<short>
    {
        public string ServerTimeZone { get; set; } = string.Empty;
        public string ApplicationVersion { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
    }
}
