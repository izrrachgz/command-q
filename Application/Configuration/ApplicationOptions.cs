namespace Application.Configuration
{
    /// <summary>Defines application-wide settings that are captured once at startup.</summary>
    public sealed class ApplicationOptions
    {
        public const string Section = "Application";
        public const string DefaultTimeZone = "America/Hermosillo";

        public string TimeZone { get; set; } = DefaultTimeZone;
    }
}
