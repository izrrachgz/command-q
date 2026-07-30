using Api.Models;
using Application.Accounts.Dtos;
using Application.AccountSessions.Dtos;

namespace Api.Interfaces
{
    /// <summary>
    /// Caches account and session data used while refreshing tokens.
    /// </summary>
    public interface IAccountSessionCache
    {
        bool TryGet(Guid accountId, out AccountSessionCacheEntry entry);

        void Set(Guid accountId, AccountModel account, AccountSessionModel session);

        void Remove(Guid accountId);

        void Clear();
    }
}
