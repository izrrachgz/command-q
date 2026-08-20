using Public;
using Common.Cqrs;
using System.Reflection;
using SqlKata.Execution;
using Common.Models.Response;
using Persistence.Abstractions;
using Localization.Abstractions;

namespace Application.Modules.Configuration.Commands
{
    internal sealed class RecordApplicationConfigurationCommandHandler(
        IWritableRepository repository,
        ITextProvider messages)
        : ICommandHandler<RecordApplicationConfigurationCommand, BasicResponse>
    {
        public async Task<BasicResponse> Handle(
            RecordApplicationConfigurationCommand request,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var entity = new Domain.Modules.Configuration.ApplicationConfiguration
            {
                ServerTimeZone = request.ServerTimeZone.Trim(),
                ApplicationVersion = ResolveVersion(request.ApplicationVersion),
                Language = request.Language.Trim(),
                Environment = request.Environment.Trim(),
                Created = now,
                Modified = now
            };

            entity.Id = await repository.Db.Query(nameof(Domain.Modules.Configuration.ApplicationConfiguration))
                .InsertGetIdAsync<short>(new
                {
                    entity.ServerTimeZone,
                    entity.ApplicationVersion,
                    entity.Language,
                    entity.Environment,
                    entity.Created,
                    entity.Modified
                }, cancellationToken: cancellationToken);

            return entity.Id > 0
                ? new(
                    true,
                    messages.Get(nameof(Messages.Created), nameof(Domain.Modules.Configuration.ApplicationConfiguration)))
                : new(
                    false,
                    messages.Get(nameof(Messages.Cannot_Create), nameof(Domain.Modules.Configuration.ApplicationConfiguration)));
        }

        private static string ResolveVersion(string? requestedVersion)
        {
            if (!string.IsNullOrWhiteSpace(requestedVersion))
                return requestedVersion.Trim();

            return Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
                ?? "unknown";
        }
    }
}
