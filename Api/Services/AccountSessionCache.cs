using Api.Models;
using Api.Interfaces;
using Common.Interfaces;
using Microsoft.Extensions.Options;
using Application.Modules.Identity.Accounts.Dtos;
using Application.Modules.Identity.AccountSessions.Dtos;

namespace Api.Services
{
    /// <summary>
    /// In-memory cache for account/session data used by refresh-token flows.
    /// </summary>
    public sealed class AccountSessionCache(ITypedCache<AccountSessionCacheEntry> accountSessionEntries, IOptions<AuthOptions> authOptions) : IAccountSessionCache
    {
        private readonly AuthOptions _authOptions = authOptions.Value;

        private static string GetKey(Guid accountId) => accountId.ToString("N");

        public bool TryGet(Guid accountId, out AccountSessionCacheEntry entry)
        {
            return accountSessionEntries.TryGet(GetKey(accountId), out entry!);
        }

        public void Set(Guid accountId, AccountModel account, AccountSessionModel session)
        {
            var expiration = TimeSpan.FromMinutes(_authOptions.RefreshCacheMinutes);

            accountSessionEntries.Set(GetKey(accountId), new AccountSessionCacheEntry(account, session), expiration, expiration * 2);
        }

        public void Remove(Guid accountId)
        {
            accountSessionEntries.Remove(GetKey(accountId));
        }

        public void Clear()
        {
            accountSessionEntries.Clear();
        }
    }
}
