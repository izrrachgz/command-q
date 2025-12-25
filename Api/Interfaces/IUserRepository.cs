using Application.Accounts.Dtos;

namespace Api.Interfaces
{
    public interface IUserRepository
    {
        Task<AccountModel?> GetByUsernameAsync(string username, CancellationToken ct);
        Task<AccountModel?> GetByIdAsync(Guid id, CancellationToken ct);
    }
}
