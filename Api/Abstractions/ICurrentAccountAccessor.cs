using Api.Models;

namespace Api.Abstractions
{
    public interface ICurrentAccountAccessor
    {
        Task<CurrentUser?> Get(CancellationToken cancellationToken = default);
    }
}
