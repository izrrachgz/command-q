using Domain.Entities;
using System.Text.Json.Serialization;

namespace Domain.Modules.Identity
{
    /// <summary>Stores persisted display, accessibility, localization and timezone preferences for an account.</summary>
    public sealed class AccountPreference : BaseEntity
    {
        public Guid AccountId { get; set; }
        public string Culture { get; set; } = string.Empty;
        public AccountTheme Theme { get; set; }
        public bool CompactNavigation { get; set; }
        public bool EnhancedFocus { get; set; }
        public bool ReduceMotion { get; set; }
        public bool ShowDenseTables { get; set; }
        public string TimeZone { get; set; } = string.Empty;
    }

    /// <summary>Supported account interface themes.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter<AccountTheme>))]
    public enum AccountTheme : short
    {
        [JsonStringEnumMemberName("dark")]
        Dark = 0,
        [JsonStringEnumMemberName("light")]
        Light = 1
    }
}
