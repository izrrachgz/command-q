using Api.Models;

namespace Api.Interfaces
{
    public interface ICurrentAccountAccessor
    {
        Task<CurrentUser?> Get(CancellationToken cancellationToken = default);
    }
}
