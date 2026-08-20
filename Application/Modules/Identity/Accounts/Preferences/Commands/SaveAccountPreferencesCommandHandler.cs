using Public;
using Common.Cqrs;
using System.Text.Json;
using SqlKata.Execution;
using Common.Models.Response;
using Persistence.Abstractions;
using Localization.Abstractions;
using Application.Modules.Identity.AccountActivity;
using Application.Modules.Identity.AccountActivity.Commands;
using Application.Modules.Identity.AccountActivity.Services;

namespace Application.Modules.Identity.Accounts.Preferences.Commands
{
    internal sealed class SaveAccountPreferencesCommandHandler(
        IWritableRepository repository,
        IAccountActivityRecorder activityRecorder,
        ITextProvider messages) : ICommandHandler<SaveAccountPreferencesCommand, BasicResponse>
    {
        public async Task<BasicResponse> Handle(SaveAccountPreferencesCommand request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var query = repository.Db.Query(nameof(AccountPreference))
                .WhereNull(nameof(AccountPreference.Deleted))
                .Where(nameof(AccountPreference.AccountId), request.AccountId);

            var values = new
            {
                Culture = request.Culture.Trim(),
                request.Theme,
                request.CompactNavigation,
                request.EnhancedFocus,
                request.ReduceMotion,
                request.ShowDenseTables,
                TimeZone = request.TimeZone.Trim(),
                Modified = now
            };

            int affected = await query.ExistsAsync(cancellationToken: cancellationToken)
                ? await query.UpdateAsync(values, cancellationToken: cancellationToken)
                : await repository.Db.Query(nameof(AccountPreference)).InsertAsync(new
                {
                    request.AccountId,
                    values.Culture,
                    values.Theme,
                    values.CompactNavigation,
                    values.EnhancedFocus,
                    values.ReduceMotion,
                    values.ShowDenseTables,
                    values.TimeZone,
                    Created = now,
                    Modified = now
                }, cancellationToken: cancellationToken);

            if (affected <= 0)
                return new(false, messages.Get(nameof(Messages.Cannot_Update), nameof(AccountPreference)));

            await activityRecorder.RecordAsync(new RecordAccountActivityCommand
            {
                AccountId = request.AccountId,
                Module = AccountActivityConstants.Modules.Accounts,
                Action = AccountActivityConstants.Actions.PreferencesChanged,
                EntityType = nameof(AccountPreference),
                EntityId = request.AccountId.ToString(),
                Message = AccountActivityConstants.Messages.PreferencesChanged,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    values.Culture,
                    values.Theme,
                    values.CompactNavigation,
                    values.EnhancedFocus,
                    values.ReduceMotion,
                    values.ShowDenseTables,
                    values.TimeZone
                })
            }, cancellationToken);

            return new(true, messages.Get(nameof(Messages.Updated), nameof(AccountPreference)));
        }
    }
}
