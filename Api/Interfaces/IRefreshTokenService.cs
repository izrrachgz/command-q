using Application.AccountSessions.Dtos;

namespace Api.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<string> CreateAsync(AccountSessionModel user);
        Task<AccountSessionModel?> ValidateAsync(string refreshToken);
        Task RotateAsync(string oldRefreshToken, string newRefreshToken);
        Task RevokeAsync(string refreshToken);
    }
}
