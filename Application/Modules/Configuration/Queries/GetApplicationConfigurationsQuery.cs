using Cqrs.Abstractions;
using Common.Models.Request;
using Common.Models.Response;
using Application.Modules.Configuration.Dtos;

namespace Application.Modules.Configuration.Queries
{
    /// <summary>Gets recorded application configuration snapshots.</summary>
    public sealed record GetApplicationConfigurationsQuery : IQuery<PagedResponse<ApplicationConfigurationModel>>
    {
        public SearchCriteria SearchCriteria { get; init; } = new();
    }
}
