using Api.Models;
using Api.Interfaces;
using Application.Accounts.Dtos;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using Application.AccountSessions.Dtos;
using Microsoft.Extensions.Caching.Memory;

namespace Api.Services
{
    /// <summary>
    /// In-memory cache for account/session data used by refresh-token flows.
    /// </summary>
    public sealed class AccountSessionCache(IMemoryCache cache, IOptions<AuthOptions> options) : IAccountSessionCache
    {
        private readonly AuthOptions _options = options.Value;

        private readonly ConcurrentDictionary<Guid, byte> _accountIds = [];

        private static string GetKey(Guid accountId) => $@"auth:account-session:{accountId:N}";

        public bool TryGet(Guid accountId, out AccountSessionCacheItem item)
        {
            return cache.TryGetValue(GetKey(accountId), out item!);
        }

        public void Set(Guid accountId, AccountModel account, AccountSessionModel session)
        {
            var expiration = TimeSpan.FromMinutes(_options.RefreshCacheMinutes);

            _accountIds[accountId] = 0;

            cache.Set(GetKey(accountId), new AccountSessionCacheItem(account, session), new MemoryCacheEntryOptions
            {
                SlidingExpiration = expiration,
                AbsoluteExpirationRelativeToNow = expiration * 2
            });
        }

        public void Remove(Guid accountId)
        {
            _accountIds.TryRemove(accountId, out _);
            cache.Remove(GetKey(accountId));
        }

        public void Clear()
        {
            foreach (var accountId in _accountIds.Keys)
                Remove(accountId);
        }
    }
}
