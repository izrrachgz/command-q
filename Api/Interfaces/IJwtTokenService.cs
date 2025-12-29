using Application.AccountSessions.Dtos;

namespace Api.Interfaces
{
    public interface IJwtTokenService
    {
        (string Token, DateTime ExpiresAt) GenerateAccessToken(AccountSessionModel session);
    }
}
