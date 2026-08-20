namespace Application.Modules.Configuration.Dtos
{
    /// <summary>Represents a recorded snapshot of effective application configuration.</summary>
    public sealed record ApplicationConfigurationModel
    {
        public short Id { get; init; }
        public string ServerTimeZone { get; init; } = string.Empty;
        public string ApplicationVersion { get; init; } = string.Empty;
        public string Language { get; init; } = string.Empty;
        public string Environment { get; init; } = string.Empty;
        public DateTime Created { get; init; }
    }
}
