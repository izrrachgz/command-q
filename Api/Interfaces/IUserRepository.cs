using Api.Models;

namespace Api.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username, CancellationToken ct);
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
    }
}
