using Common.Cqrs;
using Common.Models.Response;
using Application.Modules.Identity.Accounts.Preferences.Dtos;

namespace Application.Modules.Identity.Accounts.Preferences.Queries
{
    /// <summary>Gets the active preferences for an account.</summary>
    public sealed record GetAccountPreferencesQuery : IQuery<DetailResponse<AccountPreferenceModel>>
    {
        public Guid AccountId { get; init; }
    }
}
