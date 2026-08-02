using Application.Modules.Identity.AccountActivity.Commands;

namespace Application.Modules.Identity.AccountActivity.Services
{
    public interface IAccountActivityRecorder
    {
        Task RecordAsync(RecordAccountActivityCommand activity, CancellationToken cancellationToken = default);
    }
}
