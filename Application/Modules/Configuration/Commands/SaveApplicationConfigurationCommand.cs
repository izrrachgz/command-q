using Common.Cqrs;
using Common.Models.Response;
using Application.Modules.Configuration.Dtos;

namespace Application.Modules.Configuration.Commands
{
    /// <summary>Records an application configuration snapshot.</summary>
    public sealed record SaveApplicationConfigurationCommand : ICommand<DetailResponse<ApplicationConfigurationModel>>
    {
        public string ServerTimeZone { get; init; } = string.Empty;
        public string? ApplicationVersion { get; init; }
        public string Language { get; init; } = string.Empty;
        public string Environment { get; init; } = string.Empty;
    }
}
