using Common.Cqrs;
using Common.Models.Response;
using System.Text.Json.Serialization;

namespace Application.Modules.Identity.Accounts.Preferences.Commands
{
    /// <summary>Saves the complete preference set for an account.</summary>
    public sealed record SaveAccountPreferencesCommand : ICommand<BasicResponse>
    {
        [JsonIgnore]
        public Guid AccountId { get; init; }
        public string Culture { get; init; } = string.Empty;
        public AccountTheme Theme { get; init; }
        public bool CompactNavigation { get; init; }
        public bool EnhancedFocus { get; init; }
        public bool ReduceMotion { get; init; }
        public bool ShowDenseTables { get; init; }
        public string TimeZone { get; init; } = string.Empty;
    }
}
