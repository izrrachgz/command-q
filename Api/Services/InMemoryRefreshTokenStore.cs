using Api.Models;
using Api.Interfaces;
using System.Collections.Concurrent;

namespace Api.Services
{
    public sealed class InMemoryRefreshTokenStore : IRefreshTokenStore
    {
        private static readonly ConcurrentDictionary<string, RefreshToken> Tokens = [];

        public Task Save(RefreshToken token)
        {
            Tokens[token.Token] = token;

            return Task.CompletedTask;
        }

        public Task<RefreshToken?> Get(string refreshToken)
        {
            Tokens.TryGetValue(refreshToken, out var token);

            return Task.FromResult(token);
        }
    }
}
