using MediatR;
using Application.Modules.Identity.AccountActivity.Commands;

namespace Application.Modules.Identity.AccountActivity.Services
{
    internal sealed class AccountActivityRecorder(ISender sender) : IAccountActivityRecorder
    {
        public async Task RecordAsync(RecordAccountActivityCommand activity, CancellationToken cancellationToken = default)
        {
            await sender.Send(activity, cancellationToken);
        }
    }
}
