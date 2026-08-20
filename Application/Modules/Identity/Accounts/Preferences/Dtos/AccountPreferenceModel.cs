namespace Application.Modules.Identity.Accounts.Preferences.Dtos
{
    /// <summary>Represents an account's persisted user-interface and localization preferences.</summary>
    public sealed record AccountPreferenceModel
    {
        public string Culture { get; init; } = string.Empty;
        public AccountTheme Theme { get; init; }
        public bool CompactNavigation { get; init; }
        public bool EnhancedFocus { get; init; }
        public bool ReduceMotion { get; init; }
        public bool ShowDenseTables { get; init; }
        public string TimeZone { get; init; } = string.Empty;
    }
}
