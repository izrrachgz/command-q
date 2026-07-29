using Application.Accounts.Dtos;
using Application.AccountSessions.Dtos;

namespace Api.Models
{
    /// <summary>
    /// Account and session data cached for refresh-token flows.
    /// </summary>
    public sealed record AccountSessionCacheItem(AccountModel Account, AccountSessionModel Session);
}
