using Application.Accounts.Dtos;

namespace Api.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<string> CreateAsync(AccountModel user);
        Task<AccountModel?> ValidateAsync(string refreshToken);
        Task RotateAsync(string oldRefreshToken, string newRefreshToken);
        Task RevokeAsync(string refreshToken);
    }
}
