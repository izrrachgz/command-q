using Common.Cqrs;
using SqlKata.Execution;
using Common.Models.Response;
using Persistence.Abstractions;
using Microsoft.Extensions.Logging;
using Application.Modules.Identity.Logging;

namespace Application.Modules.Identity.AccountActivity.Commands
{
    public sealed class RecordAccountActivityCommandHandler(IWritableRepository repository, ILogger<RecordAccountActivityCommandHandler> logger) : ICommandHandler<RecordAccountActivityCommand, BasicResponse>
    {
        public async Task<BasicResponse> Handle(RecordAccountActivityCommand request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var activity = new Domain.Modules.Identity.AccountActivity
            {
                AccountId = request.AccountId,
                Module = request.Module.Trim(),
                Action = request.Action.Trim(),
                EntityType = Normalize(request.EntityType),
                EntityId = Normalize(request.EntityId),
                Message = request.Message.Trim(),
                MetadataJson = Normalize(request.MetadataJson),
                IpAddress = Normalize(request.IpAddress),
                UserAgent = Normalize(request.UserAgent),
                CreatedUtc = now,
                Created = now,
                Modified = now
            };

            try
            {
                await repository.Db.Query(nameof(Domain.Modules.Identity.AccountActivity))
                    .InsertAsync(new
                    {
                        activity.AccountId,
                        activity.Module,
                        activity.Action,
                        activity.EntityType,
                        activity.EntityId,
                        activity.Message,
                        activity.MetadataJson,
                        activity.IpAddress,
                        activity.UserAgent,
                        activity.CreatedUtc,
                        activity.Created,
                        activity.Modified
                    }, cancellationToken: cancellationToken);

                logger.AccountActivityRecorded(request.AccountId, activity.Module, activity.Action, activity.EntityType, activity.EntityId);

                return new(true, AccountActivityConstants.Messages.RecordSucceeded);
            }
            catch (Exception ex)
            {
                logger.AccountActivityRecordFailed(ex, request.AccountId, activity.Module, activity.Action);

                return new(false, AccountActivityConstants.Messages.RecordFailed);
            }
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
