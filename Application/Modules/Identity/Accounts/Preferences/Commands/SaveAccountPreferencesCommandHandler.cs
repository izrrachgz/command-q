using Public;
using Time.Extensions;
using Cqrs.Abstractions;
using System.Data;
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
        TimeProvider timeProvider,
        ITextProvider messages) : ICommandHandler<SaveAccountPreferencesCommand, BasicResponse>
    {
        public async Task<BasicResponse> Handle(SaveAccountPreferencesCommand request, CancellationToken cancellationToken)
        {
            var now = timeProvider.GetCurrentUtcDateTime();

            var query = repository.Db.Query(nameof(AccountPreference))
                .WhereNull(nameof(AccountPreference.DeletedUtc))
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
                ModifiedUtc = now
            };

            var activity = new RecordAccountActivityCommand
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
            };

            bool closeConnection = repository.Db.Connection.State != ConnectionState.Open;
            if (closeConnection)
                repository.Db.Connection.Open();

            using var transaction = repository.Db.Connection.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                var affected = await query.UpdateAsync(values, transaction, cancellationToken: cancellationToken);

                if (affected == 0)
                {
                    affected = await repository.Db.Query(nameof(AccountPreference)).InsertAsync(new
                    {
                        request.AccountId,
                        values.Culture,
                        values.Theme,
                        values.CompactNavigation,
                        values.EnhancedFocus,
                        values.ReduceMotion,
                        values.ShowDenseTables,
                        values.TimeZone,
                        CreatedUtc = now,
                        ModifiedUtc = now
                    }, transaction, cancellationToken: cancellationToken);
                }

                if (affected <= 0 || !await activityRecorder.RecordAsync(activity, transaction, cancellationToken))
                {
                    transaction.Rollback();
                    return new(false, messages.Get(nameof(Messages.Cannot_Update), nameof(AccountPreference)));
                }

                transaction.Commit();
                return new(true, messages.Get(nameof(Messages.Updated), nameof(AccountPreference)));
            }
            catch
            {
                transaction.Rollback();
                return new(false, messages.Get(nameof(Messages.Cannot_Update), nameof(AccountPreference)));
            }
            finally
            {
                if (closeConnection)
                    repository.Db.Connection.Close();
            }
        }
    }
}
