using Api.Models;
using Api.Interfaces;
using Common.Interfaces;
using Application.Accounts.Dtos;
using Microsoft.Extensions.Options;
using Application.AccountSessions.Dtos;

namespace Api.Services
{
    /// <summary>
    /// In-memory cache for account/session data used by refresh-token flows.
    /// </summary>
    public sealed class AccountSessionCache(ITypeCache<AccountSessionCacheItem> cache, IOptions<AuthOptions> options) : IAccountSessionCache
    {
        private readonly AuthOptions _options = options.Value;

        private static string GetKey(Guid accountId) => accountId.ToString("N");

        public bool TryGet(Guid accountId, out AccountSessionCacheItem item)
        {
            return cache.TryGet(GetKey(accountId), out item!);
        }

        public void Set(Guid accountId, AccountModel account, AccountSessionModel session)
        {
            var expiration = TimeSpan.FromMinutes(_options.RefreshCacheMinutes);

            cache.Set(GetKey(accountId), new AccountSessionCacheItem(account, session), expiration, expiration * 2);
        }

        public void Remove(Guid accountId)
        {
            cache.Remove(GetKey(accountId));
        }

        public void Clear()
        {
            cache.Clear();
        }
    }
}
