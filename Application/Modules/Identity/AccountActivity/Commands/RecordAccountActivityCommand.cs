using Cqrs.Abstractions;
using Common.Models.Response;

namespace Application.Modules.Identity.AccountActivity.Commands
{
    public sealed class RecordAccountActivityCommand : ICommand<BasicResponse>
    {
        public required Guid AccountId { get; set; }

        public required string Module { get; set; }

        public required string Action { get; set; }

        public string? EntityType { get; set; }

        public string? EntityId { get; set; }

        public string Message { get; set; } = string.Empty;

        public string? MetadataJson { get; set; }

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }
    }
}
