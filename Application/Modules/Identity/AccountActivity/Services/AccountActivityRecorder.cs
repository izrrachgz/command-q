using System.Data;
using SqlKata.Execution;
using Persistence.Abstractions;
using Microsoft.Extensions.Logging;
using Application.Modules.Identity.Logging;
using Application.Modules.Identity.AccountActivity.Commands;

namespace Application.Modules.Identity.AccountActivity.Services
{
    internal sealed class AccountActivityRecorder(
        IWritableRepository repository,
        TimeProvider timeProvider,
        ILogger<AccountActivityRecorder> logger) : IAccountActivityRecorder
    {
        public async Task<bool> RecordAsync(
            RecordAccountActivityCommand activity,
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default)
        {
            var now = timeProvider.GetUtcNow().UtcDateTime;
            var entity = new Domain.Modules.Identity.AccountActivity
            {
                AccountId = activity.AccountId,
                Module = activity.Module.Trim(),
                Action = activity.Action.Trim(),
                EntityType = Normalize(activity.EntityType),
                EntityId = Normalize(activity.EntityId),
                Message = activity.Message.Trim(),
                MetadataJson = Normalize(activity.MetadataJson),
                IpAddress = Normalize(activity.IpAddress),
                UserAgent = Normalize(activity.UserAgent),
                CreatedUtc = now,
                ModifiedUtc = now
            };

            try
            {
                var affected = await repository.Db.Query(nameof(Domain.Modules.Identity.AccountActivity))
                    .InsertAsync(new
                    {
                        entity.AccountId,
                        entity.Module,
                        entity.Action,
                        entity.EntityType,
                        entity.EntityId,
                        entity.Message,
                        entity.MetadataJson,
                        entity.IpAddress,
                        entity.UserAgent,
                        entity.CreatedUtc,
                        entity.ModifiedUtc
                    }, transaction, cancellationToken: cancellationToken);

                if (affected <= 0)
                    return false;

                logger.AccountActivityRecorded(activity.AccountId, entity.Module, entity.Action, entity.EntityType, entity.EntityId);
                return true;
            }
            catch (Exception exception)
            {
                logger.AccountActivityRecordFailed(exception, activity.AccountId, entity.Module, entity.Action);
                return false;
            }
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
