using Api.Models;

namespace Api.Interfaces
{
    public interface IRefreshTokenStore
    {
        Task Save(RefreshToken token);
        Task<RefreshToken?> Get(string refreshToken);
    }
}
