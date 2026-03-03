using Application.Accounts.Dtos;

namespace Api.Interfaces
{
    public interface IJwtTokenService
    {
        (string Token, DateTime ExpiresAt) GenerateAccessToken(AccountModel session, int version);
    }
}
