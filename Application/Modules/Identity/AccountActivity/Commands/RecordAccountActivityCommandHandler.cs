using Common.Cqrs;
using Common.Models.Response;
using Application.Modules.Identity.AccountActivity.Services;

namespace Application.Modules.Identity.AccountActivity.Commands
{
    internal sealed class RecordAccountActivityCommandHandler(IAccountActivityRecorder recorder) : ICommandHandler<RecordAccountActivityCommand, BasicResponse>
    {
        public async Task<BasicResponse> Handle(RecordAccountActivityCommand request, CancellationToken cancellationToken)
        {
            var recorded = await recorder.RecordAsync(request, cancellationToken: cancellationToken);
            return new(
                recorded,
                recorded ? AccountActivityConstants.Messages.RecordSucceeded : AccountActivityConstants.Messages.RecordFailed);
        }
    }
}
