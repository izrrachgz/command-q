using Domain.Modules.Identity;

namespace Api.Modules.Identity.Requests
{
    /// <summary>
    /// Describes the complete preference set to save for the authenticated account.
    /// </summary>
    public sealed record SaveAccountPreferencesRequest
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
