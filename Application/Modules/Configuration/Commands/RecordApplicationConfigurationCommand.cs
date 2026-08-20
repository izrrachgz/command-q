using Common.Cqrs;
using Common.Models.Response;

namespace Application.Modules.Configuration.Commands
{
    /// <summary>Records the effective application configuration resolved by the server at startup.</summary>
    public sealed record RecordApplicationConfigurationCommand : ICommand<BasicResponse>
    {
        public string ServerTimeZone { get; init; } = string.Empty;
        public string? ApplicationVersion { get; init; }
        public string Language { get; init; } = string.Empty;
        public string Environment { get; init; } = string.Empty;
    }
}
