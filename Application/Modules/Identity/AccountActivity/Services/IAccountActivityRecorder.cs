using System.Data;
using Application.Modules.Identity.AccountActivity.Commands;

namespace Application.Modules.Identity.AccountActivity.Services
{
    public interface IAccountActivityRecorder
    {
        Task<bool> RecordAsync(
            RecordAccountActivityCommand activity,
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default);
    }
}
