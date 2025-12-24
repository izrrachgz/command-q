using Api.Models;

namespace Api.Interfaces
{
    public interface IJwtTokenService
    {
        (string Token, DateTime ExpiresAt) GenerateAccessToken(User user);
    }
}
