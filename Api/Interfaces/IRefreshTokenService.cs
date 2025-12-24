using Api.Models;

namespace Api.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<string> CreateAsync(User user, CancellationToken cancellationToken);
        Task<User?> ValidateAsync(string refreshToken, CancellationToken cancellationToken);
        Task RotateAsync(string oldRefreshToken, string newRefreshToken);
        Task RevokeAsync(string refreshToken, CancellationToken cancellationToken);
    }
}
