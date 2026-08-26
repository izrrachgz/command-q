using Public;
using Time.Extensions;
using Cqrs.Abstractions;
using SqlKata.Execution;
using Common.Models.Response;
using Persistence.Abstractions;
using Localization.Abstractions;

namespace Application.Modules.Configuration.Commands
{
    internal sealed class RecordApplicationConfigurationCommandHandler(
        IWritableRepository repository,
        TimeProvider timeProvider,
        ITextProvider messages)
        : ICommandHandler<RecordApplicationConfigurationCommand, BasicResponse>
    {
        public async Task<BasicResponse> Handle(
            RecordApplicationConfigurationCommand request,
            CancellationToken cancellationToken)
        {
            var now = timeProvider.GetCurrentUtcDateTime();
            var entity = new Domain.Modules.Configuration.ApplicationConfiguration
            {
                ServerTimeZone = request.ServerTimeZone.Trim(),
                ApplicationVersion = request.ApplicationVersion.Trim(),
                Language = request.Language.Trim(),
                Environment = request.Environment.Trim(),
                InstanceName = request.InstanceName.Trim(),
                CreatedUtc = now,
                ModifiedUtc = now
            };

            entity.Id = await repository.Db.Query(nameof(Domain.Modules.Configuration.ApplicationConfiguration))
                .InsertGetIdAsync<int>(new
                {
                    entity.ServerTimeZone,
                    entity.ApplicationVersion,
                    entity.Language,
                    entity.Environment,
                    entity.InstanceName,
                    entity.CreatedUtc,
                    entity.ModifiedUtc
                }, cancellationToken: cancellationToken);

            return entity.Id > 0
                ? new(
                    true,
                    messages.Get(nameof(Messages.Created), nameof(Domain.Modules.Configuration.ApplicationConfiguration)))
                : new(
                    false,
                    messages.Get(nameof(Messages.Cannot_Create), nameof(Domain.Modules.Configuration.ApplicationConfiguration)));
        }

    }
}
