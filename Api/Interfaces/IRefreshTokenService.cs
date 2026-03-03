using Api.Models;
using Application.AccountSessions.Dtos;

namespace Api.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<string> Create(AccountSessionModel user);
        Task<RefreshToken?> Validate(string refreshToken);
        Task Rotate(string oldRefreshToken, string newRefreshToken);
        Task Revoke(string refreshToken);
    }
}
