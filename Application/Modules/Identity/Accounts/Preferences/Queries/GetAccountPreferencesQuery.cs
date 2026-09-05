using Cqrs.Abstractions;
using Common.Models.Response;
using Application.Modules.Identity.Accounts.Preferences.Dtos;
using Application.Abstractions;

namespace Application.Modules.Identity.Accounts.Preferences.Queries
{
    /// <summary>Gets the active preferences for an account.</summary>
    public sealed record GetAccountPreferencesQuery : IQuery<DetailResponse<AccountPreferenceModel>>, IAccountScopedRequest
    {
        public Guid AccountId { get; init; }
    }
}
